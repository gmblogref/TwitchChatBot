using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TwitchChatBot.Core.Constants;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.UI.Services
{
    public class EventSubSocketService : IEventSubService
    {
        private readonly ILogger<EventSubSocketService> _logger;
        private readonly ITwitchAlertTypesService _twitchAlertTypesService;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly SemaphoreSlim _connectLock = new(1, 1);

        private ClientWebSocket? _socket;
        private Task? _listenerTask;

        private CancellationTokenSource? _lifetimeCts;
        private CancellationTokenSource? _connectionCts;

        private int _reconnectAttempts = 0;

        private volatile bool _isStopping;
        private volatile string? _pendingReconnectUrl;
        private volatile string? _currentSessionId;

        public EventSubSocketService(
            ILogger<EventSubSocketService> logger,
            ITwitchAlertTypesService twitchAlertTypesService,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _twitchAlertTypesService = twitchAlertTypesService;
            _httpClientFactory = httpClientFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _isStopping = false;

            if (_lifetimeCts != null)
            {
                try
                {
                    _lifetimeCts.Cancel();
                }
                catch
                {
                    // ignore
                }
                _lifetimeCts.Dispose();
            }

            _lifetimeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _listenerTask = Task.Run(() => ListenerLoopAsync(_lifetimeCts.Token), _lifetimeCts.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("🛑 Stopping EventSub WebSocket...");
            _isStopping = true;

            try
            {
                _connectionCts?.Cancel();
            }
            catch
            {
                // ignore
            }

            try
            {
                _lifetimeCts?.Cancel();
            }
            catch
            {
                // ignore
            }

            await CloseSocketSafeAsync(cancellationToken);

            if (_listenerTask != null)
            {
                try
                {
                    await _listenerTask;
                }
                catch
                {
                    // ignore
                }
            }
        }

        private async Task ListenerLoopAsync(CancellationToken lifetimeToken)
        {
            while (!lifetimeToken.IsCancellationRequested && !_isStopping)
            {
                try
                {
                    DisposeConnectionResources();

                    _socket = new ClientWebSocket();
                    _connectionCts = new CancellationTokenSource();

                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken, _connectionCts.Token);

                    await ConnectAsync(linkedCts.Token);
                    await ReceiveLoopAsync(linkedCts.Token);
                }
                catch (OperationCanceledException)
                {
                    if (lifetimeToken.IsCancellationRequested || _isStopping)
                    {
                        return;
                    }

                    // Connection canceled due to session_reconnect or transient failure; loop continues.
                }
                catch (WebSocketException ex)
                {
                    _logger.LogWarning(ex, "⚠️ EventSub WebSocket exception. Will attempt reconnect.");
                    await DelayBeforeReconnectAsync(lifetimeToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ EventSub listener loop crashed. Will attempt reconnect.");
                    await DelayBeforeReconnectAsync(lifetimeToken);
                }
            }
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await _connectLock.WaitAsync(cancellationToken);
            try
            {
                if (_socket == null)
                {
                    _socket = new ClientWebSocket();
                }

                var targetUrl = !string.IsNullOrWhiteSpace(_pendingReconnectUrl)
                    ? _pendingReconnectUrl
                    : AppSettings.EventSub.Uri;

                var isReconnectUrl = !string.IsNullOrWhiteSpace(_pendingReconnectUrl);
                _pendingReconnectUrl = null;
                var uri = new Uri(targetUrl!);

                if (isReconnectUrl)
                {
                    _logger.LogInformation("🔄 Connecting to EventSub via reconnect_url: {Uri}", uri);
                }
                else
                {
                    _logger.LogInformation("🔌 Connecting to primary EventSub URI: {Uri}", uri);
                }

                await _socket.ConnectAsync(uri, cancellationToken);

                _logger.LogInformation("✅ EventSub WebSocket connected: {Uri}", uri);
            }
            finally
            {
                _connectLock.Release();
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            if (_socket == null)
            {
                return;
            }

            var buffer = new byte[8 * 1024];

            while (_socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested && !_isStopping)
            {
                var sb = new StringBuilder();
                WebSocketReceiveResult result;

                do
                {
                    result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning("EventSub socket closed by server. Status={Status} Description={Desc}",
                            result.CloseStatus,
                            result.CloseStatusDescription);

                        try
                        {
                            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                        }
                        catch
                        {
                            // ignore
                        }

                        return;
                    }

                    if (result.Count > 0)
                    {
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                }
                while (!result.EndOfMessage);

                var json = sb.ToString();

                if (string.IsNullOrWhiteSpace(json))
                {
                    continue;
                }

                await HandleMessageAsync(json);
            }
        }

        private async Task HandleMessageAsync(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("metadata", out var metadata))
                {
                    return;
                }

                var type = metadata.GetProperty("message_type").GetString();

                switch (type)
                {
                    case "session_welcome":
                        {
                            var session = root.GetProperty("payload").GetProperty("session");
                            var sessionId = session.GetProperty("id").GetString();

                            _currentSessionId = sessionId;

                            // Reset attempts only after we know Twitch gave us a valid session.
                            _reconnectAttempts = 0;

                            _logger.LogInformation(
                                "🟢 EventSub session_welcome received. SessionId={SessionId}. ReconnectAttempts reset to 0.",
                                sessionId);

                            await SubscribeToEvents(sessionId);
                            break;
                        }
                    case "session_reconnect":
                        {
                            var reconnectUrl = root.GetProperty("payload").GetProperty("session").GetProperty("reconnect_url").GetString();
                            _logger.LogInformation("🔁 EventSub session_reconnect → {Url}", reconnectUrl);

                            if (!string.IsNullOrWhiteSpace(reconnectUrl))
                            {
                                _pendingReconnectUrl = reconnectUrl;

                                try
                                {
                                    _connectionCts?.Cancel();
                                }
                                catch
                                {
                                    // ignore
                                }
                            }
                            break;
                        }
                    case "session_keepalive":
                        {
                            _logger.LogDebug("📶 keepalive");
                            break;
                        }
                    case "session_ping":
                        {
                            if (_socket != null)
                            {
                                var pong = Encoding.UTF8.GetBytes("{\"type\":\"pong\"}");
                                await _socket.SendAsync(new ArraySegment<byte>(pong), WebSocketMessageType.Text, true, CancellationToken.None);
                                _logger.LogDebug("🏓 pong");
                            }
                            break;
                        }
                    case "notification":
                        {
                            var subscriptionType = metadata.GetProperty("subscription_type").GetString();
                            var eventPayload = root.GetProperty("payload").GetProperty("event");

                            string userName = AppSettings.Ads.DefaultUserName;

                            switch (subscriptionType)
                            {
                                case TwitchEventTypes.ChannelCheer:
                                    {
                                        userName = eventPayload.GetProperty("user_name").GetString() ?? userName;
                                        var bits = eventPayload.GetProperty("bits").GetInt32();
                                        var message = eventPayload.GetProperty("message").GetString() ?? "";
                                        await _twitchAlertTypesService.HandleCheerAsync(userName, bits, message);
                                        break;
                                    }
                                case TwitchEventTypes.ChannelPointsRedemption:
                                    {
                                        userName = eventPayload.GetProperty("user_name").GetString() ?? userName;
                                        var rewardTitle = eventPayload.GetProperty("reward").GetProperty("title").GetString() ?? "";
                                        await _twitchAlertTypesService.HandleChannelPointRedemptionAsync(userName, rewardTitle);
                                        break;
                                    }
                                case TwitchEventTypes.HypeTrainBegin:
                                    {
                                        await _twitchAlertTypesService.HandleHypeTrainAsync();
                                        break;
                                    }
                                case TwitchEventTypes.HypeTrainEnd:
                                    {
                                        await _twitchAlertTypesService.HandleHypeTrainEndAsync(ParseHypeTrainEndEvent(eventPayload));
                                        break;
                                    }
                                case TwitchEventTypes.ChannelFollow:
                                    {
                                        var followerName =
                                            eventPayload.TryGetProperty("user_name", out var uName) && uName.ValueKind == JsonValueKind.String
                                                ? uName.GetString()
                                                : (eventPayload.TryGetProperty("user_login", out var uLogin) && uLogin.ValueKind == JsonValueKind.String
                                                    ? uLogin.GetString()
                                                    : AppSettings.Ads.DefaultUserName);

                                        await _twitchAlertTypesService.HandleFollowAsync(followerName ?? AppSettings.Ads.DefaultUserName);
                                        break;
                                    }
                                default:
                                    {
                                        _logger.LogDebug("Unhandled subscription type: {SubscriptionType}", subscriptionType);
                                        break;
                                    }
                            }
                            break;
                        }
                    default:
                        {
                            _logger.LogDebug("Unhandled message_type: {Type}", type);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to parse or handle EventSub payload.");
            }
        }

        private async Task SubscribeToEvents(string? sessionId)
        {
            if (sessionId == null)
            {
                _logger.LogWarning("Session ID was null. Cannot subscribe to EventSub.");
                return;
            }

            if (!string.Equals(sessionId, _currentSessionId, StringComparison.Ordinal))
            {
                _logger.LogInformation("Skipping subscribe; stale session {Old} (current {New})", sessionId, _currentSessionId);
                return;
            }

            var token = AppSettings.Auth.TWITCH_ACCESS_TOKEN;
            var clientId = AppSettings.Auth.TWITCH_CLIENT_ID;
            var broadcasterId = AppSettings.Twitch.TWITCH_USER_ID;

            if (string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(broadcasterId))
            {
                _logger.LogWarning("❌ Missing required Twitch credentials.");
                return;
            }

            var eventTypes = new[]
            {
                TwitchEventTypes.ChannelCheer,
                TwitchEventTypes.ChannelPointsRedemption,
                TwitchEventTypes.ChannelPointsRedemptionUpdate,
                TwitchEventTypes.HypeTrainBegin,
                TwitchEventTypes.HypeTrainProgress,
                TwitchEventTypes.HypeTrainEnd,
                TwitchEventTypes.ChannelFollow
            };

            var http = _httpClientFactory.CreateClient("twitch-eventsub");
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            http.DefaultRequestHeaders.Add("Client-Id", clientId);

            var tokenUserId = await GetTokenUserIdAsync(http);
            if (string.IsNullOrEmpty(tokenUserId))
            {
                _logger.LogWarning("❌ Could not resolve token user id via /validate. channel.follow will fail.");
            }

            foreach (var type in eventTypes)
            {
                if (!string.Equals(sessionId, _currentSessionId, StringComparison.Ordinal))
                {
                    _logger.LogInformation("Stopping subscriptions; session changed mid-loop.");
                    break;
                }

                object condition;
                string version = type switch
                {
                    TwitchEventTypes.ChannelFollow => "2",
                    TwitchEventTypes.HypeTrainBegin => "2",
                    TwitchEventTypes.HypeTrainProgress => "2",
                    TwitchEventTypes.HypeTrainEnd => "2",
                    _ => "1"
                };

                if (type == TwitchEventTypes.ChannelFollow)
                {
                    if (string.IsNullOrEmpty(tokenUserId))
                    {
                        _logger.LogWarning("Skipping channel.follow; token user id not available.");
                        continue;
                    }

                    condition = new
                    {
                        broadcaster_user_id = broadcasterId,
                        moderator_user_id = tokenUserId
                    };
                }
                else
                {
                    condition = new { broadcaster_user_id = broadcasterId };
                }

                var body = new
                {
                    type,
                    version,
                    condition,
                    transport = new
                    {
                        method = "websocket",
                        session_id = sessionId
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                try
                {
                    var response = await http.PostAsync(AppSettings.EventSub.PostSubscriptionsUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("✅ Subscribed to {Type} (v{Version})", type, version);
                    }
                    else
                    {
                        var details = await response.Content.ReadAsStringAsync();

                        if ((int)response.StatusCode == 409)
                        {
                            _logger.LogInformation("ℹ️ Already subscribed to {Type}: {Body}", type, details);
                        }
                        else
                        {
                            _logger.LogWarning("❌ Failed to subscribe to {Type}: {Status} {Body}", type, response.StatusCode, details);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Exception while subscribing to {Type}", type);
                }
            }
        }

        private async Task<string?> GetTokenUserIdAsync(HttpClient http)
        {
            try
            {
                using var resp = await http.GetAsync(AppSettings.EventSub.Validate);
                if (!resp.IsSuccessStatusCode)
                {
                    return null;
                }

                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                return doc.RootElement.TryGetProperty("user_id", out var uid) ? uid.GetString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate token for user id.");
                return null;
            }
        }

        private async Task DelayBeforeReconnectAsync(CancellationToken cancellationToken)
        {
            _reconnectAttempts++;

            var delay = Math.Min(10000, 1000 * (int)Math.Pow(2, _reconnectAttempts));
            _logger.LogInformation("🔁 Reconnecting EventSub WebSocket in {Seconds}s...", delay / 1000);

            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Reconnect canceled.");
            }
        }

        private async Task CloseSocketSafeAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_socket != null &&
                    (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseReceived))
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "shutdown", cancellationToken);
                }
            }
            catch
            {
                // ignore
            }

            try
            {
                _socket?.Dispose();
            }
            catch
            {
                // ignore
            }

            _socket = null;
        }

        private HypeTrainEnd ParseHypeTrainEndEvent(JsonElement eventPayload)
        {
            var hypeTrainEnd = new HypeTrainEnd();

            hypeTrainEnd.Level = eventPayload.TryGetProperty("level", out var lvlProp) &&
                lvlProp.ValueKind == JsonValueKind.Number
                ? lvlProp.GetInt32()
                : 0;

            if (eventPayload.TryGetProperty("top_contributions", out var contributions) &&
                contributions.ValueKind == JsonValueKind.Array)
            {
                foreach (var contribution in contributions.EnumerateArray())
                {
                    var type = contribution.TryGetProperty("type", out var typeProp) &&
                               typeProp.ValueKind == JsonValueKind.String
                        ? typeProp.GetString()
                        : string.Empty;

                    var user = contribution.TryGetProperty("user_name", out var userProp) &&
                               userProp.ValueKind == JsonValueKind.String
                        ? userProp.GetString()
                        : (contribution.TryGetProperty("user_login", out var loginProp) &&
                           loginProp.ValueKind == JsonValueKind.String
                            ? loginProp.GetString()
                            : AppSettings.Ads.DefaultUserName);

                    var total = contribution.TryGetProperty("total", out var totalProp) &&
                                totalProp.ValueKind == JsonValueKind.Number
                        ? totalProp.GetInt32()
                        : 0;

                    if (string.Equals(type, "bits", StringComparison.OrdinalIgnoreCase))
                    {
                        if (total > hypeTrainEnd.TopCheerBits)
                        {
                            hypeTrainEnd.TopCheerBits = total;
                            hypeTrainEnd.TopCheerUser = user;
                        }
                    }
                    else if (string.Equals(type, "subs", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(type, "subscription", StringComparison.OrdinalIgnoreCase))
                    {
                        if (total > hypeTrainEnd.TopGiftsubs)
                        {
                            hypeTrainEnd.TopGiftsubs = total;
                            hypeTrainEnd.TopGiftsubUser = user;
                        }
                    }
                }
            }

            return hypeTrainEnd;
        }

        private void DisposeConnectionResources()
        {
            try
            {
                _connectionCts?.Cancel();
            }
            catch
            {
                // ignore
            }

            try
            {
                _connectionCts?.Dispose();
            }
            catch
            {
                // ignore
            }

            _connectionCts = null;

            try
            {
                _socket?.Dispose();
            }
            catch
            {
                // ignore
            }

            _socket = null;
        }
    }
}