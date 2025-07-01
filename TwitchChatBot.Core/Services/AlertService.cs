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
        private System.Threading.Timer? _adTimer;

        private IUiBridge? _uiBridge; // <- Now nullable and injected via setter

        public AlertService(ILogger<AlertService> logger, IWebSocketServer webSocketServer)
        {
            _logger = logger;
            _webSocketServer = webSocketServer;
        }

        public void SetUiBridge(IUiBridge bridge)
        {
            _uiBridge = bridge;
        }

        public void EnqueueAlert(string message, string? mediaPath = null)
        {
            _alertQueue.Enqueue(new AlertItem
            {
                Message = message,
                Media = mediaPath
            });

            ProcessQueue();
        }

        public void StartAdTimer(TimeSpan interval)
        {
            _adTimer = new System.Threading.Timer(_ =>
            {
                EnqueueAlert("📺 Ads starting soon! Use !ads to avoid surprises!", null);
                _logger.LogInformation("⏰ Auto !ads alert triggered.");
            }, null, interval, interval);
        }

        public void StopAdTimer()
        {
            _adTimer?.Dispose();
            _adTimer = null;
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
                    message = alert.Message,
                    media = alert.Media
                };

                _webSocketServer.BroadcastAsync(payload);
                _logger.LogInformation("📤 Alert sent: {Message}, Media: {Media}", alert.Message, alert.Media);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to process alert.");
            }
            finally
            {
                _isProcessing = false;

                if (_alertQueue.Count > 0)
                    ProcessQueue();
            }
        }
    }
}
