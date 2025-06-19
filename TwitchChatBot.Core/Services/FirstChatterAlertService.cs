using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;
using TwitchLib.Client.Interfaces;

namespace TwitchChatBot.Core.Services
{
    public class FirstChatterAlertService : IFirstChatterAlertService
    {
        private readonly ILogger<FirstChatterAlertService> _logger;
        private readonly IFirstChatterMediaRepository _firstChatterMediaRepository;
        private readonly IExcludedUsersRepository _excludedUsersRepo;
        private readonly IAlertService _alertService;
        private readonly ITwitchClientWrapper _twitchClientWrapper;

        private const string UniversalMessage =
            "🎉 @[userName] welcome to the Ballpark! Find your seat and enjoy the game. 🎉";

        public FirstChatterAlertService(
            ILogger<FirstChatterAlertService> logger,
            IFirstChatterMediaRepository firstChatterMediaRepository,
            IExcludedUsersRepository excludedUsersRepo,
            IAlertService alertService,
            ITwitchClientWrapper twitchClientWrapper)
        {
            _logger = logger;
            _firstChatterMediaRepository = firstChatterMediaRepository;
            _excludedUsersRepo = excludedUsersRepo;
            _alertService = alertService;
            _twitchClientWrapper = twitchClientWrapper;
        }

        public async Task HandleFirstChatAsync(string username, string displayName)
        {
            if (await _excludedUsersRepo.IsUserExcludedAsync(username))
            {
                _logger.LogInformation("⛔ Ignored first chatter alert for excluded user or non eligible user: {User}", username);
                return;
            }

            if (_firstChatterMediaRepository.HasAlreadyChatted(username))
            {
                return;
            }

            _firstChatterMediaRepository.MarkAsChatted(username);

            var media = await _firstChatterMediaRepository.GetFirstChatterMediaAsync(username);
            var message = UniversalMessage.Replace("[userName]", displayName);

            if (media != null)
            {
                _alertService.EnqueueAlert(message, media);
            }
            else
            {
                _twitchClientWrapper.SendMessage(AppSettings.TWITCH_CHANNEL!, message);
            }
        }

        public void ClearFirstChatters()
        {
            _firstChatterMediaRepository.ClearFirstChatters();
        }
    }
}
