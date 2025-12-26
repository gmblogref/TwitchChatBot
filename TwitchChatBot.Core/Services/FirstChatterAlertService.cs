using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Core.Utilities;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class FirstChatterAlertService : IFirstChatterAlertService
    {
        private readonly ILogger<FirstChatterAlertService> _logger;
        private readonly IFirstChatterMediaRepository _firstChatterMediaRepository;
        private readonly IExcludedUsersService _excludedUsersService;
        private readonly IAlertService _alertService;
        private readonly IWatchStreakService _watchStreakService;
        private readonly IAlertHistoryService _alertHistoryService;
        private readonly Action<string, string> _sendMessage;
        private HashSet<string> _firstChatters = [];

        private const string UniversalMessage =
            "🎉 @[userName] welcome to the Ballpark! Find your seat and enjoy the game. 🎉";

        public FirstChatterAlertService(
            ILogger<FirstChatterAlertService> logger,
            IFirstChatterMediaRepository firstChatterMediaRepository,
            IExcludedUsersService excludedUsersService,
            IAlertService alertService,
            IWatchStreakService watchStreakService,
            IAlertHistoryService alertHistoryService,
            Action<string, string> sendMessage)
        {
            _logger = logger;
            _firstChatterMediaRepository = firstChatterMediaRepository;
            _excludedUsersService = excludedUsersService;
            _alertService = alertService;
            _watchStreakService = watchStreakService;
            _alertHistoryService = alertHistoryService;
            _sendMessage = sendMessage;
        }

        public void ClearFirstChatters()
        {
            _firstChatters?.Clear();
        }

        public async Task<bool> HandleFirstChatAsync(string username, string displayName, bool isReplay = false)
        {
            if (!isReplay)
            {
                if (await _excludedUsersService.IsUserExcludedAsync(username))
                {
                    _logger.LogInformation("⛔ Ignored first chatter alert for excluded user or non eligible user: {User}", username);
                    return false;
                }

                if (HasAlreadyChatted(username))
                {
                    return false;
                }

                await _watchStreakService.MarkAttendanceAsync(username);
                MarkAsChatted(username);
            }

            var mediaFileName = await _firstChatterMediaRepository.GetFirstChatterMediaAsync(username);
            var message = UniversalMessage.Replace("[userName]", displayName);

            if (mediaFileName != null && CanPlayChatterMedia(username))
            {
                _alertHistoryService.Add(new AlertHistoryEntry
                {
                    Type = AlertHistoryType.First,
                    Display = $"First Chatter: {username}",
                    Username = username,
                    DisplayName = displayName,
                });

                _alertService.EnqueueAlert(message, CoreHelperMethods.ToPublicMediaPath(mediaFileName));

                return true;
            }
            else
            {
                _sendMessage(AppSettings.TWITCH_CHANNEL!, message);
            }

            return false;
        }

        private bool HasAlreadyChatted(string username)
        {
            return _firstChatters?.Contains(username) ?? false;
        }

        private void MarkAsChatted(string username)
        {
            _firstChatters.Add(username);
        }

        private async Task<bool> IsEligibleForFirstChatAsync(string username)
        {
            return await _firstChatterMediaRepository.IsEligibleForFirstChatAsync(username);
        }

        private bool CanPlayChatterMedia(string username)
        {
            // Move this list to Config if it gets bigger than 2 users
            if (!string.Equals(username, "whynot7058", StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(username, "whynot7058", StringComparison.OrdinalIgnoreCase) &&
                DateTime.Now.DayOfWeek == DayOfWeek.Wednesday))
            {
                return true;
            }

            return false;
        }
    }
}