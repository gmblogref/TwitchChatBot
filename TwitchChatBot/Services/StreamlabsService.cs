using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.UI.Services
{
    public class StreamlabsSocketService : IStreamlabsService
    {
        private readonly ILogger<StreamlabsSocketService> _logger;
        private readonly ITwitchAlertTypesService _twitchAlertTypesService;
        private readonly ClientWebSocket _socket = new();
        private readonly CancellationTokenSource _cts = new();
        private Task? _listenTask;
        private Action<string, string?>? _alertHandler;
        private int _reconnectAttempts = 0;

        public StreamlabsSocketService(ILogger<StreamlabsSocketService> logger, ITwitchAlertTypesService twitchAlertTypesService)
        {
            _logger = logger;
            _twitchAlertTypesService = twitchAlertTypesService;
        }

        public void Start(Action<string, string?> onFollowAlert)
        {
            _alertHandler = onFollowAlert;

            var token = AppSettings.Streamlabs.STREAMLABS_SOCKET_TOKEN;
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogError("STREAMLABS_SOCKET_TOKEN is missing.");
                return;
            }

            var uri = new Uri(string.Format(AppSettings.Streamlabs.Url!, token));
            _listenTask = Task.Run(() => ConnectAndListenAsync(uri, _cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
            _socket.Dispose();
        }

        private async Task ConnectAndListenAsync(Uri uri, CancellationToken cancellationToken)
        {
            try
            {
                await _socket.ConnectAsync(uri, cancellationToken);
                _logger.LogInformation("✅ Connected to Streamlabs WebSocket.");

                var buffer = new byte[8192];

                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning("⚠️ Streamlabs WebSocket closed. Reconnecting...");
                        await AttemptReconnectAsync(uri, cancellationToken);
                        return;
                    }

                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (json.StartsWith("42"))
                    {
                        HandlePayload(json[2..]); // Skip the "42" prefix
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in Streamlabs WebSocket connection.");
                await AttemptReconnectAsync(uri, cancellationToken);
            }
        }

        private void HandlePayload(string rawJson)
        {
            try
            {
                var doc = JsonDocument.Parse(rawJson);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() != 2) return;

                var eventType = root[0].GetString();
                var data = root[1];

                if (eventType != "event") return;

                var type = data.GetProperty("type").GetString();
                if (type != "follow") return;

                foreach (var message in data.GetProperty("message").EnumerateArray())
                {
                    var username = message.GetProperty("name").GetString();
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        _logger.LogInformation($"🟢 Follow alert received from {username}");

                        _twitchAlertTypesService.HandleFollowAsync(username);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to parse follow alert payload.");
            }
        }

        private async Task AttemptReconnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            _reconnectAttempts++;
            var delay = Math.Min(10000, 1000 * (int)Math.Pow(2, _reconnectAttempts));
            _logger.LogInformation($"🔄 Reconnecting to Streamlabs in {delay / 1000}s...");

            await Task.Delay(delay, cancellationToken);
            if (!cancellationToken.IsCancellationRequested)
            {
                _socket.Dispose();
                await ConnectAndListenAsync(uri, cancellationToken);
            }
        }
    }
}