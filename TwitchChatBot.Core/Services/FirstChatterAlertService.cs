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
        private readonly IHelixLookupService _helixLookupService;
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
            IHelixLookupService helixLookupService,
            Action<string, string> sendMessage)
        {
            _logger = logger;
            _firstChatterMediaRepository = firstChatterMediaRepository;
            _excludedUsersService = excludedUsersService;
            _alertService = alertService;
            _watchStreakService = watchStreakService;
            _alertHistoryService = alertHistoryService;
            _helixLookupService = helixLookupService;
            _sendMessage = sendMessage;
        }

        public void ClearFirstChatters()
        {
            _firstChatters?.Clear();
        }

        public async Task<bool> HandleFirstChatAsync(string userId, string username, string displayName, bool isReplay = false)
        {
            if (!isReplay)
            {
                if (await _excludedUsersService.IsUserExcludedAsync(userId, username))
                {
                    _logger.LogInformation("⛔ Ignored first chatter alert for excluded user or non eligible user: {User}", username);
                    return false;
                }

                if (HasAlreadyChatted(userId))
                {
                    return false;
                }

                await _watchStreakService.MarkAttendanceAsync(userId, username);
                MarkAsChatted(userId);
            }

            var firstChatterMediaItem = await _firstChatterMediaRepository.GetFirstChatterMediaAsync(userId, username);
            var message = UniversalMessage.Replace("[userName]", displayName);

            if (firstChatterMediaItem != null && firstChatterMediaItem.Media != null && CanPlayChatterMedia(firstChatterMediaItem))
            {
                _alertHistoryService.Add(new AlertHistoryEntry
                {
                    Type = AlertHistoryType.First,
                    UserId = userId,
                    Display = $"First Chatter: {username}",
                    Username = username,
                    DisplayName = displayName,
                });

                _alertService.EnqueueAlert(message, CoreHelperMethods.ToPublicMediaPath(firstChatterMediaItem.Media));

                return true;
            }
            else
            {
                _sendMessage(AppSettings.TWITCH_CHANNEL, message);
            }

            return false;
        }

        private bool HasAlreadyChatted(string userId)
        {
            return _firstChatters?.Contains(userId) ?? false;
        }

        private void MarkAsChatted(string userId)
        {
            _firstChatters.Add(userId);
        }

        private bool CanPlayChatterMedia(FirstChatterMediaItem firstChatterMediaItem)
        {
            if (firstChatterMediaItem == null)
            {
                return false;
            }

            if (firstChatterMediaItem.AllowedDaysOfWeek == null ||
                firstChatterMediaItem.AllowedDaysOfWeek.Count == 0)
            {
                return true;
            }

            return firstChatterMediaItem.AllowedDaysOfWeek.Contains(DateTime.Now.DayOfWeek);
        }
    }
}