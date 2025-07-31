using Microsoft.Extensions.Logging;
using SocketIOClient;
using System.Text.Json;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.UI.Services
{
    public class StreamlabsSocketService : IStreamlabsService
    {
        private readonly ILogger<StreamlabsSocketService> _logger;
        private readonly ITwitchAlertTypesService _twitchAlertTypesService;
        private SocketIOClient.SocketIO? _socket;

        public StreamlabsSocketService(ILogger<StreamlabsSocketService> logger, ITwitchAlertTypesService twitchAlertTypesService)
        {
            _logger = logger;
            _twitchAlertTypesService = twitchAlertTypesService;
        }

        public void Start(Action<string, string?> onFollowAlert)
        {
            var token = AppSettings.Streamlabs.STREAMLABS_SOCKET_TOKEN;
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogError("STREAMLABS_SOCKET_TOKEN is missing.");
                return;
            }

            var uri = new Uri(string.Format(AppSettings.Streamlabs.Url!, token));
            _socket = new SocketIOClient.SocketIO(uri.ToString(), new SocketIOOptions
            {
                Reconnection = true,
                ReconnectionAttempts = int.MaxValue,
                ReconnectionDelay = 2000,
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            });

            _socket.OnConnected += (_, _) => _logger.LogInformation("✅ Connected to Streamlabs Socket.IO");
            _socket.OnDisconnected += (_, reason) => _logger.LogWarning($"⚠️ Disconnected from Streamlabs: {reason}");
            _socket.OnError += (sender, ex) => _logger.LogError(ex, "❌ Streamlabs Socket.IO error");
            _socket.OnReconnectAttempt += (sender, attempt) => _logger.LogInformation($"🔁 Reconnect attempt #{attempt}");
            _socket.On("event", async response =>
            {
                try
                {
                    var payload = response.GetValue<string>();

                    var followEvent = JsonSerializer.Deserialize<StreamlabsFollowWrapper>(payload);

                    if (followEvent?.Type == "follow" && followEvent.Message?.Count > 0)
                    {
                        foreach (var msg in followEvent.Message)
                        {
                            var username = msg?.Name?.Trim();
                            if (!string.IsNullOrWhiteSpace(username))
                            {
                                _logger.LogInformation($"🟢 Follow alert received from {username}");
                                await _twitchAlertTypesService.HandleFollowAsync(username);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Streamlabs event received, but was not a valid follow payload.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to deserialize or process Streamlabs follow event.");
                }
            });

            _socket.ConnectAsync();
        }

        public void Stop()
        {
            if (_socket is not null)
            {
                _ = _socket.DisconnectAsync();
                _socket.Dispose();
                _logger.LogInformation("⛔ Streamlabs Socket.IO stopped.");
            }
        }
    }
}
