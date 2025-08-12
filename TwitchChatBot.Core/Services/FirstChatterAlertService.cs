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
            Action<string, string> sendMessage)
        {
            _logger = logger;
            _firstChatterMediaRepository = firstChatterMediaRepository;
            _excludedUsersService = excludedUsersService;
            _alertService = alertService;
            _watchStreakService = watchStreakService;
            _sendMessage = sendMessage;
        }

        public void ClearFirstChatters()
        {
            _firstChatters?.Clear();
        }

        public async Task HandleFirstChatAsync(string username, string displayName)
        {
            if (await _excludedUsersService.IsUserExcludedAsync(username))
            {
                _logger.LogInformation("⛔ Ignored first chatter alert for excluded user or non eligible user: {User}", username);
                return;
            }

            if (HasAlreadyChatted(username))
            {
                return;
            }

            await _watchStreakService.MarkAttendanceAsync(username);
            MarkAsChatted(username);

            var mediaFileName = await _firstChatterMediaRepository.GetFirstChatterMediaAsync(username);
            var message = UniversalMessage.Replace("[userName]", displayName);

            if (mediaFileName != null)
            {
                _alertService.EnqueueAlert(message, CoreHelperMethods.ToPublicMediaPath(mediaFileName));
            }
            else
            {
                _sendMessage(AppSettings.TWITCH_CHANNEL!, message);
            }
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
    }
}