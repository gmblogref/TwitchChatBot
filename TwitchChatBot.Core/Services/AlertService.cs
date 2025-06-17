using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TwitchChatBot.Core.Models;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Services
{
    public class AlertService : IAlertService
    {
        private readonly ILogger<AlertService> _logger;
        private readonly IWebSocketServer _webSocketServer;
        private readonly Queue<AlertItem> _alertQueue = new();
        private bool _isProcessing = false;
        private System.Threading.Timer? _adTimer;

        public AlertService(ILogger<AlertService> logger, IWebSocketServer webSocketServer)
        {
            _logger = logger;
            _webSocketServer = webSocketServer;
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

        public void EnqueueAlert(string message, string? mediaPath = null)
        {
            _alertQueue.Enqueue(new AlertItem { Message = message, Media = mediaPath });
            ProcessQueue();
        }

        public void HandleChannelPointAlert(string username, string rewardTitle, string userInput)
        {
            try
            {
                var mediaMap = _mediaMap.Channel_Points;
                var textMap = _mediaMap.Channel_Points_Text;

                // Handle media match
                var matchedMedia = mediaMap?.Tiers?.FirstOrDefault(t =>
                    t.Title?.Equals(rewardTitle, StringComparison.OrdinalIgnoreCase) == true);

                if (!string.IsNullOrEmpty(matchedMedia?.Media))
                {
                    _logger.LogInformation("🎯 Channel point media match: {RewardTitle}", rewardTitle);
                    EnqueueAlert("", matchedMedia.Media);
                }

                // Handle text match
                var matchedText = textMap?.Tiers?.FirstOrDefault(t =>
                    t.Title?.Equals(rewardTitle, StringComparison.OrdinalIgnoreCase) == true);

                if (!string.IsNullOrEmpty(matchedText?.Message))
                {
                    var message = matchedText.Message.Replace("[userName]", username);
                    _logger.LogInformation("📝 Channel point text match: {RewardTitle} => {Message}", rewardTitle, message);
                    EnqueueAlert(message, null);
                }

                if (matchedMedia == null && matchedText == null)
                {
                    _logger.LogInformation("ℹ️ No matching alert for channel point reward: {RewardTitle}", rewardTitle);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling channel point redemption for {RewardTitle}", rewardTitle);
            }
        }

        public void HandleCheerAlert(string username, int bits, string chatMessage)
        {
            var message = $"🎉 {username} cheered {bits} bits! {chatMessage}";
            EnqueueAlert(message);
        }

        public void HandleFollowAlert(string username)
        {
            var message = $"🟢 {username} just followed the channel! Welcome to the Ballpark!";
            EnqueueAlert(message);
        }

        public void HandleGiftSubAlert(string gifter, string recipient)
        {
            var message = $"🎁 {gifter} gifted a sub to {recipient}!";
            EnqueueAlert(message);
        }

        public void HandleHypeTrainAlert()
        {
            var message = $"🚂 All aboard the Hype Train! Let’s keep it going!";
            EnqueueAlert(message);
        }

        public void HandleMysteryGiftAlert(string username, int subCount)
        {
            var message = $"🎁 {username} is dropping {subCount} gift subs!";
            EnqueueAlert(message);
        }

        public void HandleRaidAlert(string username, int viewers)
        {
            var message = $"🚨 {username} is raiding with {viewers} viewers!";
            EnqueueAlert(message);
        }

        public void HandleResubAlert(string username, int months, string resubMessage)
        {
            var message = $"💜 {username} resubscribed for {months} months! {resubMessage}";
            EnqueueAlert(message);
        }

        public void HandleSubAlert(string username)
        {
            var message = $"💜 {username} just subscribed! Welcome to the Ballpark!";
            EnqueueAlert(message);
        }

        private void ProcessQueue()
        {
            if (_isProcessing || _alertQueue.Count == 0)
                return;

            _isProcessing = true;
            var alert = _alertQueue.Dequeue();

            try
            {
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
                _logger.LogError(ex, "❌ Failed to send alert.");
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
