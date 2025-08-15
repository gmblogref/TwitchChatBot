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
        private ClientWebSocket _socket;
        private CancellationTokenSource _cts;
        private Task? _listenerTask;
        private int _reconnectAttempts = 0;
        private volatile string? _currentSessionId;

        public EventSubSocketService(
            ILogger<EventSubSocketService> logger,
            ITwitchAlertTypesService twitchAlertTypesService)
        {
            _logger = logger;
            _twitchAlertTypesService = twitchAlertTypesService;
            _socket = new ClientWebSocket();
            _cts = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _listenerTask = Task.Run(() => ConnectAsync(cancellationToken));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("🛑 Stopping EventSub WebSocket...");
            _cts.Cancel();
            return Task.CompletedTask;
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                _socket = new ClientWebSocket();
                _cts = new CancellationTokenSource();

                var uri = new Uri(AppSettings.EventSub.Uri!);
                await _socket.ConnectAsync(uri, cancellationToken);
                _logger.LogInformation("✅ Connected to Twitch EventSub WebSocket.");

                var buffer = new byte[8192];
                var sb = new StringBuilder();

                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                _logger.LogWarning("⚠️ EventSub WebSocket closed.");
                                await AttemptReconnectAsync(_cts.Token);
                                return;
                            }

                            sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        }
                        while (!result.EndOfMessage);

                        var json = sb.ToString();
                        sb.Clear();

                        await HandleMessageAsync(json);
                    }
                    catch (WebSocketException ex)
                    {
                        _logger.LogError(ex, "❌ WebSocket exception caught.");
                        await AttemptReconnectAsync(_cts.Token);
                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Receive loop canceled.");
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Unexpected error in receive loop.");
                        await AttemptReconnectAsync(_cts.Token);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in EventSub WebSocket connection.");
                await AttemptReconnectAsync(cancellationToken);
            }
            finally
            {
                if (_socket.State != WebSocketState.Open)
                {
                    _logger.LogWarning("🔌 WebSocket no longer open. Attempting reconnect...");
                    await AttemptReconnectAsync(cancellationToken);
                }
            }
        }

        private async Task HandleMessageAsync(string json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("metadata", out var metadata))
                {
                    var type = metadata.GetProperty("message_type").GetString();

                    switch (type)
                    {
                        case "session_welcome":
                            {
                                var session = root.GetProperty("payload").GetProperty("session");
                                var sessionId = session.GetProperty("id").GetString();

                                _currentSessionId = sessionId;  // track current session
                                await SubscribeToEvents(sessionId);
                                break;
                            }

                        case "session_reconnect":
                            {
                                var reconnectUrl = root.GetProperty("payload").GetProperty("session").GetProperty("reconnect_url").GetString();
                                _logger.LogInformation("🔁 EventSub session_reconnect → {Url}", reconnectUrl);

                                // Close and reconnect to the new URL; DO NOT resubscribe
                                try { _cts.Cancel(); } catch { }
                                _ = Task.Run(async () =>
                                {
                                    // fresh CTS and socket
                                    _socket?.Dispose();
                                    _socket = new ClientWebSocket();
                                    _cts = new CancellationTokenSource();
                                    await _socket.ConnectAsync(new Uri(reconnectUrl!), _cts.Token);
                                    _logger.LogInformation("✅ Reconnected to EventSub via reconnect_url.");
                                    // receive loop will resume in ConnectAsync if you structure it that way,
                                    // or you can continue this loop similarly.
                                });
                                break;
                            }

                        case "session_keepalive":
                            // nothing to send; Twitch is telling us it's alive
                            _logger.LogDebug("📶 keepalive");
                            break;

                        case "session_ping":
                            // respond with PONG
                            var pong = Encoding.UTF8.GetBytes("{\"type\":\"pong\"}");
                            await _socket.SendAsync(new ArraySegment<byte>(pong), WebSocketMessageType.Text, true, _cts.Token);
                            _logger.LogDebug("🏓 pong");
                            break;

                        case "notification":
                            string userName = "someone";
                            var subscriptionType = metadata.GetProperty("subscription_type").GetString();
                            var eventPayload = root.GetProperty("payload").GetProperty("event");
                            var subTier = "1000";

                            switch (subscriptionType)
                            {
                                case TwitchEventTypes.ChannelCheer:
                                    userName = eventPayload.GetProperty("user_name").GetString() ?? userName;
                                    var bits = eventPayload.GetProperty("bits").GetInt32();
                                    var message = eventPayload.GetProperty("message").GetString() ?? "";
                                    await _twitchAlertTypesService.HandleCheerAsync(userName, bits, message);
                                    break;

                                case TwitchEventTypes.ChannelPointsRedemption:
                                    userName = eventPayload.GetProperty("user_name").GetString() ?? userName;
                                    var rewardTitle = eventPayload.GetProperty("reward").GetProperty("title").GetString() ?? "";
                                    await _twitchAlertTypesService.HandleChannelPointRedemptionAsync(userName, rewardTitle);
                                    break;

                                case TwitchEventTypes.ChannelRaid:
                                    var raiderName = eventPayload.GetProperty("from_broadcaster_user_login").GetString() ?? "someone";
                                    var viewers = eventPayload.GetProperty("viewers").GetInt32();
                                    await _twitchAlertTypesService.HandleRaidAsync(raiderName, viewers);
                                    break;

                                case TwitchEventTypes.ChannelSubscribe:
                                    userName = eventPayload.GetProperty("user_name").GetString() ?? userName;
                                    subTier = GetSubTier(eventPayload);

                                    await _twitchAlertTypesService.HandleSubscriptionAsync(userName, subTier);
                                    break;

                                case TwitchEventTypes.ChannelSubscriptionGift:
                                    userName = eventPayload.GetProperty("user_name").GetString() == null ? userName : "someone";
                                    var recipient = eventPayload.GetProperty("recipient_user_name").GetString() ?? "someone";
                                    subTier = GetSubTier(eventPayload);
                                    var totalGifts = eventPayload.TryGetProperty("total", out var totalVal) ? totalVal.GetInt32() : 1;

                                    if (totalGifts > 1)
                                    {
                                        // Mystery gift bundle
                                        await _twitchAlertTypesService.HandleSubMysteryGiftAsync(userName, totalGifts, subTier);
                                    }
                                    else
                                    {
                                        await _twitchAlertTypesService.HandleSubGiftAsync(userName, recipient, subTier!);
                                    }
                                    break;

                               case TwitchEventTypes.ChannelSubscriptionMessage:
                                    userName = eventPayload.GetProperty("user_name").GetString() ?? userName;
                                    var months = eventPayload.GetProperty("cumulative_months").GetInt32();
                                    var resubMessage = eventPayload.GetProperty("message").GetProperty("text").GetString() ?? "";
                                    subTier = GetSubTier(eventPayload);
                                    
                                    await _twitchAlertTypesService.HandleResubAsync(userName, months, resubMessage, subTier);
                                    break;

                                case TwitchEventTypes.HypeTrainBegin:
                                    await _twitchAlertTypesService.HandleHypeTrainAsync();
                                    break;

                                case TwitchEventTypes.ChannelFollow:
                                    // v2 fields: user_name, user_login, user_id, followed_at, broadcaster_*
                                    var followerName = eventPayload.TryGetProperty("user_name", out var uName) && uName.ValueKind == JsonValueKind.String
                                        ? uName.GetString()
                                        : (eventPayload.TryGetProperty("user_login", out var uLogin) && uLogin.ValueKind == JsonValueKind.String
                                            ? uLogin.GetString()
                                            : "someone");

                                    await _twitchAlertTypesService.HandleFollowAsync(followerName ?? "someone");
                                    break;
                                default:
                                    _logger.LogDebug($"Unhandled subscription type: {subscriptionType}");
                                    break;
                            }
                            break;

                        default:
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

            // guard against stale session id
            if (!string.Equals(sessionId, _currentSessionId, StringComparison.Ordinal))
            {
                _logger.LogInformation("Skipping subscribe; stale session {Old} (current {New})", sessionId, _currentSessionId);
                return;
            }

            var token = AppSettings.TWITCH_ACCESS_TOKEN;
            var clientId = AppSettings.TWITCH_CLIENT_ID;
            var broadcasterId = AppSettings.TWITCH_USER_ID;

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
                TwitchEventTypes.ChannelPointsRedemptionUpdate, // optional
                TwitchEventTypes.HypeTrainBegin,
                TwitchEventTypes.HypeTrainProgress,             // optional
                TwitchEventTypes.HypeTrainEnd,                  // optional
                TwitchEventTypes.ChannelRaid,
                TwitchEventTypes.ChannelSubscribe,
                TwitchEventTypes.ChannelSubscriptionGift,
                TwitchEventTypes.ChannelSubscriptionMessage,
                TwitchEventTypes.ChannelFollow                    // NEW — requires v2 + moderator_user_id
            };

            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            http.DefaultRequestHeaders.Add("Client-Id", clientId);

            var tokenUserId = await GetTokenUserIdAsync(http);
            if (string.IsNullOrEmpty(tokenUserId))
            {
                _logger.LogWarning("❌ Could not resolve token user id via /validate. channel.follow will fail.");
            }

            foreach (var type in eventTypes)
            {
                // if session changed mid-loop, stop
                if (!string.Equals(sessionId, _currentSessionId, StringComparison.Ordinal))
                {
                    _logger.LogInformation("Stopping subscriptions; session changed mid-loop.");
                    break;
                }

                string version = "1";
                object condition;

                if (type == TwitchEventTypes.ChannelRaid)
                {
                    condition = new { to_broadcaster_user_id = broadcasterId };
                }
                else if (type == TwitchEventTypes.ChannelFollow)
                {
                    if (string.IsNullOrEmpty(tokenUserId))
                    {
                        _logger.LogWarning("Skipping channel.follow; token user id not available.");
                        continue;
                    }
                    version = "2";
                    condition = new
                    {
                        broadcaster_user_id = broadcasterId,
                        moderator_user_id = tokenUserId  // <-- use token owner id (must be a mod)
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
                            _logger.LogInformation("ℹ️ Already subscribed to {Type}: {Body}", type, details);
                        else
                            _logger.LogWarning("❌ Failed to subscribe to {Type}: {Status} {Body}", type, response.StatusCode, details);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Exception while subscribing to {type}");
                }
            }
        }

        private async Task AttemptReconnectAsync(CancellationToken cancellationToken)
        {
            _reconnectAttempts++;
            var delay = Math.Min(10000, 1000 * (int)Math.Pow(2, _reconnectAttempts));
            _logger.LogInformation($"🔁 Reconnecting EventSub WebSocket in {delay / 1000}s...");
            try
            {
                await Task.Delay(delay, cancellationToken);
                await ConnectAsync(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Reconnect canceled.");
            }
        }

        private async Task<string?> GetTokenUserIdAsync(HttpClient http)
        {
            try
            {
                using var resp = await http.GetAsync(AppSettings.EventSub.Validate);
                if (!resp.IsSuccessStatusCode) return null;

                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                return doc.RootElement.TryGetProperty("user_id", out var uid) ? uid.GetString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate token for user id.");
                return null;
            }
        }

        private string GetSubTier(JsonElement eventPayload)
        {
            var result = eventPayload.TryGetProperty("tier", out var t) && t.ValueKind == JsonValueKind.String
                                        ? t.GetString()
                                        : "1000";

            return result ?? "1000";
        }
    }
}