using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Utilities.Contracts;
using TwitchChatBot.Data;
using TwitchChatBot.Data.Contracts;

namespace TwitchChatBot.Core.Services
{
    public class HandleAlertTypesService : IHandleAlertTypesService
    {
        private readonly ILogger<HandleAlertTypesService> _logger;
        private readonly ITwitchAlertMediaRepository _twitchAlertMediaRepository;
        private ICoreHelperMethods _coreHelperMethods;

        public HandleAlertTypesService(
            ILogger<HandleAlertTypesService> logger, 
            TwitchAlertMediaRepository twitchAlertMediaRepository, 
            ICoreHelperMethods coreHelperMethods)
        {
            _logger = logger;
            _twitchAlertMediaRepository = twitchAlertMediaRepository;
            _coreHelperMethods = coreHelperMethods;
        }

        public async Task HandleCheerAsync(string username, int bits, string message, IAlertService alertService)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "Cheer", username);

            var msg = $"🎉 {username} cheered {bits} bits!";

            var cheers = await _twitchAlertMediaRepository.GetCheerMapAsync();
            var tier = cheers!.tiers.FirstOrDefault(x => x.Min == bits);

            alertService.EnqueueAlert(msg, tier?.Media ?? cheers.Default);
        }

        public async Task HandleHypeTrainAsync(IAlertService alertService)
        {
            _logger.LogInformation("📣 Alert triggered: {Type}", "HypeTrain");

            var media = await _twitchAlertMediaRepository.GetHypeTrainMediaAsync();
            var msg = $"🚂 All aboard the Hype Train! Let's keep it going!";

            alertService.EnqueueAlert(msg, media![_coreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);
        }

        public async Task HandleRaidAsync(string username, int viewers, IAlertService alertService)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "Raid", username);

            var media = await _twitchAlertMediaRepository.GetRaidMediaAsync();
            var msg = $"🚨 {username} is raiding with {viewers} viewers!";

            alertService.EnqueueAlert(msg, media![_coreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);
        }

        public async Task HandleSubscriptionAsync(string username, IAlertService alertService)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "ubscription", username);

            var media = await _twitchAlertMediaRepository.GetSubscriptionMediaAsync();
            var msg = $"💜 {username} just subscribed!";

            alertService.EnqueueAlert(msg, media![_coreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);
        }

        public async Task HandleSubGiftAsync(string username, string recipient, IAlertService alertService)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "GiftSub", username);

            var media = await _twitchAlertMediaRepository.GetSubgiftMediaAsync();
            var msg = $"🎁 {username} gifted a sub to {recipient}!";

            alertService.EnqueueAlert(msg, media![_coreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);
        }

        public async Task HandleResubAsync(string username, int months, string userMessage, IAlertService alertService)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "ReSub", username);

            var media = await _twitchAlertMediaRepository.GetResubMediaAsync();
            var msg = $"💜 {username} resubscribed for {months} months! {userMessage}";

            alertService.EnqueueAlert(msg, media![_coreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);
        }

        public async Task HandleSubMysteryGiftAsync(string username, int numOfSubs, IAlertService alertService)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "Many Gift Subs", username);

            var msg = $"🎁 {username} is dropping {numOfSubs} gift subs!";

            var giftSubMedia = await _twitchAlertMediaRepository.GetSubMysteryGiftMapAsync();
            var tier = giftSubMedia!.tiers.FirstOrDefault(x =>
                (x.Match == "gte" && numOfSubs >= x.Value) || (x.Match == "eq" && numOfSubs == x.Value));

            alertService.EnqueueAlert(msg, tier?.Media ?? giftSubMedia.Default);
        }

        public async Task HandleChannelPointRedemptionAsync(string username, string rewardTitle, IAlertService alertService)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "Channel Point", username);

            var channelPointMedia = await _twitchAlertMediaRepository.GetChannelPointsMapAsync();
            var channelPointTextMedia = await _twitchAlertMediaRepository.GetChannelPointsTextMapAsync();

            if (channelPointMedia!.Tiers.Exists(x => x.Title.ToLower() == rewardTitle.ToLower()))
            {
                alertService.EnqueueAlert("", (channelPointMedia!.Tiers.First(x => x.Title.ToLower() == rewardTitle.ToLower()).Media));
            }
            else if (channelPointTextMedia!.Tiers.Exists(x => x.Title.ToLower() == rewardTitle.ToLower()))
            {
                alertService.EnqueueAlert(channelPointTextMedia!.Tiers.First(x => x.Title.ToLower() == rewardTitle.ToLower()).Message, null);
            }
        }
    }
}