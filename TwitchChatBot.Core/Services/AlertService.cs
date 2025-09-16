using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class AlertService : IAlertService
    {
        private readonly ILogger<AlertService> _logger;
        private readonly IWebSocketServer _webSocketServer;
        private readonly Queue<AlertItem> _alertQueue = new();
        private bool _isProcessing = false;

        private IUiBridge? _uiBridge; // <- Now nullable and injected via setter

        public AlertService(ILogger<AlertService> logger, IWebSocketServer webSocketServer)
        {
            _logger = logger;
            _webSocketServer = webSocketServer;

            // Subscribe to done events from WebSocketServer
            _webSocketServer.OnClientDone += HandleClientDone;
        }

        public void SetUiBridge(IUiBridge bridge)
        {
            _uiBridge = bridge;
        }

        // Overload for default alert type
        public void EnqueueAlert(string message, string? mediaPath = null)
        {
            EnqueueAlert("alert", message, mediaPath);
        }

        // New overload that lets you specify type
        public void EnqueueAlert(string type, string message, string? mediaPath = null)
        {
            _alertQueue.Enqueue(new AlertItem
            {
                Type = type,
                Message = message,
                Media = mediaPath
            });

            ProcessQueue();
        }

        private void ProcessQueue()
        {
            if (_isProcessing || _alertQueue.Count == 0)
                return;

            _isProcessing = true;

            try
            {
                var alert = _alertQueue.Dequeue();

                var payload = new
                {
                    type = alert.Type,
                    message = alert.Message,
                    media = alert.Media
                };

                _webSocketServer.BroadcastAsync(payload);
                _logger.LogInformation("📤 Alert sent: {Type}, {Message}, Media: {Media}", alert.Type, alert.Message, alert.Media);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to process alert.");
            }
        }

        private void HandleClientDone()
        {
            _logger.LogInformation("✅ Client reported alert finished");
            _isProcessing = false;
            ProcessQueue();
        }
    }
}