using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Utilities;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class TwitchAlertTypesService : ITwitchAlertTypesService
    {
        private readonly ILogger<TwitchAlertTypesService> _logger;
        private readonly ITwitchAlertMediaRepository _twitchAlertMediaRepository;
        private readonly IAlertService _alertService;

        public TwitchAlertTypesService(
            ILogger<TwitchAlertTypesService> logger, 
            ITwitchAlertMediaRepository twitchAlertMediaRepository,
            IAlertService alertService)
        {
            _logger = logger;
            _twitchAlertMediaRepository = twitchAlertMediaRepository;
            _alertService = alertService;
        }

        public async Task HandleCheerAsync(string username, int bits, string message)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "Cheer", username);

            var msg = $"🎉 {username} cheered {bits} bits!";

            var cheers = await _twitchAlertMediaRepository.GetCheerMapAsync();

            Tier? tier = null;

            if (bits == 38 || bits == 350)
            {
                tier = cheers!.tiers.FirstOrDefault(x => x.Min == bits);
            }
            else
            {
                tier = cheers!.tiers
                    .Where(x => bits >= x.Min)
                    .OrderByDescending(x => x.Min)
                    .FirstOrDefault();
            }

            EnqueueAlertWithMedia(msg, tier?.Media ?? cheers.Default);
        }

        public async Task HandleFollowAsync(string username)
        {
            var media = await _twitchAlertMediaRepository.GetFollowMediaAsync();
            if (media == null || media.Count == 0)
            {
                _logger.LogWarning("⚠️ No follow media found.");
                return;
            }

            var selected = media[CoreHelperMethods.GetRandomNumberForMediaSelection(media.Count)];
            var msg = $"🎉 {username} just followed the channel!";
            EnqueueAlertWithMedia(msg, selected);
        }
        
        public async Task HandleHypeTrainAsync()
        {
            _logger.LogInformation("📣 Alert triggered: {Type}", "HypeTrain");

            var media = await _twitchAlertMediaRepository.GetHypeTrainMediaAsync();
            var msg = $"🚂 All aboard the Hype Train! Let's keep it going!";

            EnqueueAlertWithMedia(msg, media![CoreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);
        }

        public async Task HandleRaidAsync(string username, int viewers)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "Raid", username);

            var media = await _twitchAlertMediaRepository.GetRaidMediaAsync();
            var msg = $"🚨 {username} is raiding with {viewers} viewers!";

            EnqueueAlertWithMedia(msg, media![CoreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);
        }

        public async Task HandleSubscriptionAsync(string username)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "ubscription", username);

            var media = await _twitchAlertMediaRepository.GetSubscriptionMediaAsync();
            var msg = $"💜 {username} just subscribed!";

            EnqueueAlertWithMedia(msg, media![CoreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);
        }

        public async Task HandleSubGiftAsync(string username, string recipient)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "GiftSub", username);

            var media = await _twitchAlertMediaRepository.GetSubgiftMediaAsync();
            var msg = $"🎁 {username} gifted a sub to {recipient}!";

            EnqueueAlertWithMedia(msg, media![CoreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);
        }

        public async Task HandleResubAsync(string username, int months, string userMessage)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "ReSub", username);

            var media = await _twitchAlertMediaRepository.GetResubMediaAsync();
            var msg = $"💜 {username} resubscribed for {months} months! {userMessage}";

            EnqueueAlertWithMedia(msg, media![CoreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);
        }

        public async Task HandleSubMysteryGiftAsync(string username, int numOfSubs)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "Many Gift Subs", username);

            var msg = $"🎁 {username} is dropping {numOfSubs} gift subs!";

            var giftSubMedia = await _twitchAlertMediaRepository.GetSubMysteryGiftMapAsync();
            var tier = giftSubMedia!.tiers.FirstOrDefault(x =>
                (x.Match == "gte" && numOfSubs >= x.Value) || (x.Match == "eq" && numOfSubs == x.Value));

            EnqueueAlertWithMedia(msg, tier?.Media ?? giftSubMedia.Default);
        }

        public async Task HandleChannelPointRedemptionAsync(string username, string rewardTitle)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "Channel Point", username);

            var channelPointMedia = await _twitchAlertMediaRepository.GetChannelPointsMapAsync();
            var channelPointTextMedia = await _twitchAlertMediaRepository.GetChannelPointsTextMapAsync();

            if (channelPointMedia!.Tiers.Exists(x => x.Title.ToLower() == rewardTitle.ToLower()))
            {
                EnqueueAlertWithMedia($"{username} has redeemed {rewardTitle}", (channelPointMedia!.Tiers.First(x => x.Title.ToLower() == rewardTitle.ToLower()).Media));
            }
            else if (channelPointTextMedia!.Tiers.Exists(x => x.Title.ToLower() == rewardTitle.ToLower()))
            {
                var fixedMessage = CoreHelperMethods.ReplacePlaceholders(channelPointTextMedia!.Tiers.First(x => x.Title.ToLower() == rewardTitle.ToLower()).Message, username);
                _alertService.EnqueueAlert(fixedMessage, null);
            }
        }

        private void EnqueueAlertWithMedia(string message, string mediaPath)
        {
            if (string.IsNullOrWhiteSpace(mediaPath))
                return;

            _alertService.EnqueueAlert(message, CoreHelperMethods.ToPublicMediaPath(mediaPath));
        }
    }
}