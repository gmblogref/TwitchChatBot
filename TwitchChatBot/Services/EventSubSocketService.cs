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
        private readonly IAlertService _alertService;
        private readonly ILogger<EventSubSocketService> _logger;
        private ClientWebSocket _socket;
        private CancellationTokenSource _cts;
        private Task? _listenerTask;
        private int _reconnectAttempts = 0;
        private System.Threading.Timer? _heartbeatTimer;

        public EventSubSocketService(IAlertService alertService, ILogger<EventSubSocketService> logger)
        {
            _alertService = alertService;
            _logger = logger;
            _socket = new ClientWebSocket();
            _cts = new CancellationTokenSource();
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("🛑 Stopping EventSub WebSocket...");
            _cts.Cancel();
            _heartbeatTimer?.Dispose();
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _listenerTask = Task.Run(() => ConnectAsync(cancellationToken));
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
                    var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning("⚠️ EventSub WebSocket closed.");
                        await AttemptReconnectAsync(cancellationToken);
                        return;
                    }

                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await HandleMessageAsync(json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in EventSub WebSocket connection.");
                await AttemptReconnectAsync(cancellationToken);
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

                        var username = eventPayload.GetProperty("user_name").GetString() ??
                                       eventPayload.GetProperty("broadcaster_user_name").GetString() ??
                                       eventPayload.GetProperty("from_broadcaster_user_name").GetString() ??
                                       "someone";

                        switch (subscriptionType)
                        {
                            case TwitchEventTypes.ChannelCheer:
                                var bits = eventPayload.GetProperty("bits").GetInt32();
                                var cheerMessage = eventPayload.TryGetProperty("message", out var msgVal) ? msgVal.GetString() ?? "" : "";
                                _alertService.HandleCheerAlert(username, bits, cheerMessage);
                                break;

                            case TwitchEventTypes.ChannelPointsRedemption:
                                var rewardTitle = eventPayload.GetProperty("reward").GetProperty("title").GetString() ?? "";
                                var userInput = eventPayload.TryGetProperty("user_input", out var inputVal) ? inputVal.GetString() ?? "" : "";
                                _alertService.HandleChannelPointAlert(username, rewardTitle, userInput);
                                break;

                            case TwitchEventTypes.HypeTrainBegin:
                                _alertService.HandleHypeTrainAlert();
                                break;

                            case TwitchEventTypes.ChannelRaid:
                                var viewers = eventPayload.GetProperty("viewers").GetInt32();
                                _alertService.HandleRaidAlert(username, viewers);
                                break;

                            case TwitchEventTypes.ChannelSubscribe:
                                _alertService.HandleSubAlert(username);
                                break;

                            case TwitchEventTypes.ChannelSubscriptionGift:
                                var recipient = eventPayload.GetProperty("recipient_user_name").GetString() ?? "someone";
                                _alertService.HandleGiftSubAlert(username, recipient);
                                break;

                            case TwitchEventTypes.ChannelSubscriptionMessage:
                                var months = eventPayload.GetProperty("cumulative_months").GetInt32();
                                var message = eventPayload.GetProperty("message").GetProperty("text").GetString() ?? "";
                                _alertService.HandleResubAlert(username, months, message);
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
                _logger.LogWarning(ex, "⚠️ Failed to parse EventSub payload.");
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

            using var http = new HttpClient();

            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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

                var response = await http.PostAsync("https://api.twitch.tv/helix/eventsub/subscriptions", content);

                _logger.LogInformation(response.IsSuccessStatusCode
                    ? $"✅ Subscribed to {type}"
                    : $"❌ Failed to subscribe to {type}: {response.StatusCode}");
            }
        }


        private void StartHeartbeat(int timeoutSeconds)
        {
            var interval = TimeSpan.FromSeconds(timeoutSeconds - 5);
            _heartbeatTimer = new System.Threading.Timer(_ =>
            {
                var ping = Encoding.UTF8.GetBytes("{\"type\":\"ping\"}");
                _socket.SendAsync(new ArraySegment<byte>(ping), WebSocketMessageType.Text, true, CancellationToken.None);
            }, null, interval, interval);

            _logger.LogDebug("💓 Heartbeat started.");
        }

        private async Task AttemptReconnectAsync(CancellationToken cancellationToken)
        {
            _reconnectAttempts++;
            var delay = Math.Min(10000, 1000 * (int)Math.Pow(2, _reconnectAttempts));
            _logger.LogInformation($"🔁 Reconnecting EventSub WebSocket in {delay / 1000}s...");
            await Task.Delay(delay, cancellationToken);
            await ConnectAsync(cancellationToken);
        }
    }
}
