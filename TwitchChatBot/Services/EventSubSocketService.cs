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
        private readonly IAlertService _alertService;
        private readonly ITwitchAlertTypesService _handleAlertTypesService;
        private ClientWebSocket _socket;
        private CancellationTokenSource _cts;
        private Task? _listenerTask;
        private System.Threading.Timer? _heartbeatTimer;
        private int _reconnectAttempts = 0;

        public EventSubSocketService(
            ILogger<EventSubSocketService> logger,
            IAlertService alertService,
            ITwitchAlertTypesService handleAlertTypesService)
        {
            _logger = logger;
            _alertService = alertService;
            _handleAlertTypesService = handleAlertTypesService;
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
            _heartbeatTimer?.Dispose();
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

                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            _logger.LogWarning("⚠️ EventSub WebSocket closed.");
                            _heartbeatTimer?.Dispose();
                            await AttemptReconnectAsync(cancellationToken);
                            return;
                        }

                        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await HandleMessageAsync(json);
                    }
                    catch (WebSocketException ex)
                    {
                        _logger.LogError(ex, "❌ WebSocket exception caught.");
                        _heartbeatTimer?.Dispose();
                        await AttemptReconnectAsync(cancellationToken);
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Unexpected error in receive loop.");
                        _heartbeatTimer?.Dispose();
                        await AttemptReconnectAsync(cancellationToken);
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
                    _heartbeatTimer?.Dispose();
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
                    if (type == "session_welcome")
                    {
                        var sessionId = root.GetProperty("payload").GetProperty("session").GetProperty("id").GetString();
                        var keepAlive = root.GetProperty("payload").GetProperty("session").GetProperty("keepalive_timeout_seconds").GetInt32();

                        StartHeartbeat(keepAlive);
                        await SubscribeToEvents(sessionId);
                    }
                    else if (type == "notification")
                    {
                        var subscriptionType = metadata.GetProperty("subscription_type").GetString();
                        var eventPayload = root.GetProperty("payload").GetProperty("event");
                        var username = eventPayload.GetProperty("user_name").GetString() ?? "someone";

                        switch (subscriptionType)
                        {
                            case TwitchEventTypes.ChannelCheer:
                                var bits = eventPayload.GetProperty("bits").GetInt32();
                                var message = eventPayload.GetProperty("message").GetString() ?? "";
                                await _handleAlertTypesService.HandleCheerAsync(username, bits, message, _alertService);
                                break;

                            case TwitchEventTypes.ChannelPointsRedemption:
                                var rewardTitle = eventPayload.GetProperty("reward").GetProperty("title").GetString() ?? "";
                                await _handleAlertTypesService.HandleChannelPointRedemptionAsync(username, rewardTitle, _alertService);
                                break;

                            case TwitchEventTypes.ChannelRaid:
                                var viewers = eventPayload.GetProperty("viewers").GetInt32();
                                await _handleAlertTypesService.HandleRaidAsync(username, viewers, _alertService);
                                break;

                            case TwitchEventTypes.ChannelSubscribe:
                                await _handleAlertTypesService.HandleSubscriptionAsync(username, _alertService);
                                break;

                            case TwitchEventTypes.ChannelSubscriptionGift:
                                var recipient = eventPayload.GetProperty("recipient_user_name").GetString() ?? "someone";
                                await _handleAlertTypesService.HandleSubGiftAsync(username, recipient, _alertService);
                                break;

                            case TwitchEventTypes.ChannelSubscriptionMessage:
                                var months = eventPayload.GetProperty("cumulative_months").GetInt32();
                                var resubMessage = eventPayload.GetProperty("message").GetProperty("text").GetString() ?? "";
                                await _handleAlertTypesService.HandleResubAsync(username, months, resubMessage, _alertService);
                                break;

                            case TwitchEventTypes.HypeTrainBegin:
                                await _handleAlertTypesService.HandleHypeTrainAsync(_alertService);
                                break;

                            default:
                                _logger.LogDebug($"Unhandled subscription type: {subscriptionType}");
                                break;
                        }
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

            var token = AppSettings.TWITCH_ACCESS_TOKEN;
            var clientId = AppSettings.TWITCH_CLIENT_ID;
            var broadcasterId = AppSettings.TWITCH_USER_ID;

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(broadcasterId))
            {
                _logger.LogWarning("❌ Missing required Twitch credentials.");
                return;
            }

            var eventTypes = new[]
            {
                TwitchEventTypes.ChannelCheer,
                TwitchEventTypes.ChannelPointsRedemption,
                TwitchEventTypes.HypeTrainBegin,
                TwitchEventTypes.ChannelRaid,
                TwitchEventTypes.ChannelSubscribe,
                TwitchEventTypes.ChannelSubscriptionGift,
                TwitchEventTypes.ChannelSubscriptionMessage
            };

            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            http.DefaultRequestHeaders.Add("Client-ID", clientId);

            foreach (var type in eventTypes)
            {
                var body = new
                {
                    type,
                    version = "1",
                    condition = new { broadcaster_user_id = broadcasterId },
                    transport = new
                    {
                        method = "websocket",
                        session_id = sessionId
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                try
                {
                    var response = await http.PostAsync("https://api.twitch.tv/helix/eventsub/subscriptions", content);
                    _logger.LogInformation(response.IsSuccessStatusCode
                        ? $"✅ Subscribed to {type}"
                        : $"❌ Failed to subscribe to {type}: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Exception while subscribing to {type}");
                }
            }
        }


        private void StartHeartbeat(int timeoutSeconds)
        {
            var interval = TimeSpan.FromSeconds(timeoutSeconds - 5);
            _heartbeatTimer = new System.Threading.Timer(async _ =>
            {
                if (_socket?.State == WebSocketState.Open)
                {
                    try
                    {
                        var ping = Encoding.UTF8.GetBytes("{\"type\":\"ping\"}");
                        await _socket.SendAsync(new ArraySegment<byte>(ping), WebSocketMessageType.Text, true, CancellationToken.None);
                        _logger.LogDebug("💓 Sent heartbeat ping.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Heartbeat failed.");
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ Skipped heartbeat: WebSocket is not open.");
                }
            }, null, interval, interval);
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
    }
}