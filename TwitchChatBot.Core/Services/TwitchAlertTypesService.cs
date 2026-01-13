using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
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
        private readonly ITtsService _tsService;
        private readonly IAlertHistoryService _alertHistoryService;

        public TwitchAlertTypesService(
            ILogger<TwitchAlertTypesService> logger, 
            ITwitchAlertMediaRepository twitchAlertMediaRepository,
            IAlertService alertService,
            ITtsService tsService,
            IAlertHistoryService alertHistoryService)
        {
            _logger = logger;
            _twitchAlertMediaRepository = twitchAlertMediaRepository;
            _alertService = alertService;
            _tsService = tsService;
            _alertHistoryService = alertHistoryService;
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

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.TwitchCheer,
                Display = $"Cheer: {username} +{bits}",
                Username = username,
                Bits = bits,
                Message = message
            });

            EnqueueAlertWithMedia(msg, tier?.Media ?? cheers.Default);

            var voice = AppSettings.Voices.Cheer ?? AppSettings.TTS.DefaultSpeaker ?? "Matthew";

            var text = !string.IsNullOrWhiteSpace(message)
                ? CoreHelperMethods.ForTts(message)
                : CoreHelperMethods.RenderTemplate(
                    AppSettings.Templates.CheerNoMessage ?? "{username} cheered {bits} bits!",
                    new Dictionary<string, string?>
                    {
                        ["username"] = username,
                        ["bits"] = bits.ToString()
                    });

            if (!string.IsNullOrWhiteSpace(text))
                await _tsService.SpeakAsync(text, voice);
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

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.TwitchFollow,
                Display = $"Follow: {username}",
                Username = username
            });

            EnqueueAlertWithMedia(msg, selected);

            var voice = AppSettings.Voices.Follow ?? AppSettings.TTS.DefaultSpeaker ?? "Matthew";
            var template = AppSettings.Templates.Follow ?? "{username} has decided to join the team!";
            var text = CoreHelperMethods.RenderTemplate(template, new Dictionary<string, string?>
            {
                ["username"] = username
            });

            await _tsService.SpeakAsync(text, voice);
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

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.TwitchRaid,
                Display = $"Raid: {username} ({viewers})",
                Username = username,
                Viewers = viewers
            });

            EnqueueAlertWithMedia(msg, media![CoreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);

            var voice = AppSettings.Voices.Raid ?? AppSettings.TTS.DefaultSpeaker ?? "Matthew";
            var template = AppSettings.Templates.Raid ?? "{raider} is storming in with {viewers} viewers!";
            var text = CoreHelperMethods.RenderTemplate(template, new Dictionary<string, string?>
            {
                ["raider"] = username,
                ["viewers"] = viewers.ToString()
            });

            await _tsService.SpeakAsync(text, voice);
        }

        public async Task HandleSubscriptionAsync(string username, string subTier)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "Subscription", username);

            var media = await _twitchAlertMediaRepository.GetSubscriptionMediaAsync();
            var msg = $"💜 {username} just subscribed!";

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.TwitchSub,
                Display = $"Sub: {username} (Tier {subTier})",
                Username = username,
                Tier = subTier
            });

            EnqueueAlertWithMedia(msg, media![CoreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);

            var voice = AppSettings.Voices.Subscribe ?? AppSettings.TTS.DefaultSpeaker ?? "Matthew";
            var template = AppSettings.Templates.SubNoMessage ?? "{username} just subscribed at Tier {tier} —welcome!";
            var text = CoreHelperMethods.RenderTemplate(template, new Dictionary<string, string?>
            {
                ["username"] = username,
                ["tier"] = GetNiceTierString(subTier)
            });

            await _tsService.SpeakAsync(text, voice);
        }

        public async Task HandleSubGiftAsync(string username, string recipient, string subTier)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "GiftSub", username);

            var media = await _twitchAlertMediaRepository.GetSubgiftMediaAsync();
            var msg = $"🎁 {username} gifted a sub to {recipient}!";

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.TwitchSubGift,
                Display = $"Gift: {username} → {recipient} (Tier {subTier})",
                Gifter = username,
                Recipient = recipient,
                Tier = subTier
            });

            EnqueueAlertWithMedia(msg, media![CoreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);

            var voice = AppSettings.Voices.SingleGiftSub ?? AppSettings.TTS.DefaultSpeaker;
            var template = AppSettings.Templates.SingleGiftSub ?? "{username} has given {recipient} a (Tier {tier} game ticket. {recipient} thank your sub daddy";
            var text = CoreHelperMethods.RenderTemplate(template, new Dictionary<string, string?>
            {
                ["username"] = username,
                ["recipient"] = recipient,
                ["tier"] = GetNiceTierString(subTier)
            });

            await _tsService.SpeakAsync(text, voice);
        }

        public async Task HandleResubAsync(string username, int months, string userMessage, string subTier)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "ReSub", username);

            var media = await _twitchAlertMediaRepository.GetResubMediaAsync();
            var msg = $"💜 {username} resubscribed for {months} months! {userMessage}";

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.TwitchSubMessage,
                Display = $"ReSub: {username} ({months} months, Tier {subTier})",
                Username = username,
                Months = months,
                Tier = subTier,
                Message = msg
            });

            EnqueueAlertWithMedia(msg, media![CoreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);

            var voice = AppSettings.Voices.SubscriptionMessage ?? AppSettings.TTS.DefaultSpeaker ?? "Matthew";

            var text = !string.IsNullOrWhiteSpace(userMessage)
                ? CoreHelperMethods.ForTts(userMessage)
                : CoreHelperMethods.RenderTemplate(
                    AppSettings.Templates.ReSub ?? "{username} is back for {months} months at tier {tier} —thank you!",
                    new Dictionary<string, string?>
                    {
                        ["username"] = username,
                        ["months"] = months.ToString(),
                        ["tier"] = GetNiceTierString(subTier)
                    });

            await _tsService.SpeakAsync(text, voice);
        }

        public async Task HandleSubMysteryGiftAsync(string username, int numOfSubs, string subTier)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "Many Gift Subs", username);

            username = username ?? "someone";
            var msg = $"🎁 {username} is dropping {numOfSubs} gift subs!";

            var giftSubMedia = await _twitchAlertMediaRepository.GetSubMysteryGiftMapAsync();
            var tier = giftSubMedia!.tiers.FirstOrDefault(x =>
                (x.Match == "gte" && numOfSubs >= x.Value) || (x.Match == "eq" && numOfSubs == x.Value));

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.TwitchMysteryGift,
                Display = $"MysteryGift: {username} x{numOfSubs} (Tier {subTier})",
                Gifter = username,
                Count = numOfSubs,
                Tier = subTier
            });

            EnqueueAlertWithMedia(msg, tier?.Media ?? giftSubMedia.Default);

            var voice = AppSettings.Voices.GiftSubs ?? AppSettings.TTS.DefaultSpeaker ?? "Matthew";
            var template = AppSettings.Templates.MysteryGift ?? "{username} just dropped {numOfSubs} tier {tier} gift subs! Thank your gift sub daddy!";
            var text = CoreHelperMethods.RenderTemplate(template, new Dictionary<string, string?>
            {
                ["username"] = username,
                ["numOfSubs"] = numOfSubs.ToString(),
                ["tier"] = GetNiceTierString(subTier)
            });

            await _tsService.SpeakAsync(text, voice);
        }

        public async Task HandleChannelPointRedemptionAsync(string username, string rewardTitle)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}", "Channel Point", username);

            var channelPointMedia = await _twitchAlertMediaRepository.GetChannelPointsMapAsync();
            var channelPointTextMedia = await _twitchAlertMediaRepository.GetChannelPointsTextMapAsync();

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.ChannelPoint,
                Display = $"CP: {username} - {rewardTitle}",
                Username = username,
                RewardTitle = rewardTitle
            });

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

        public async Task HandleWatchStreakNoticeAsync(string username, int streakCount, string? userMessage)
        {
            _logger.LogInformation("📣 Alert triggered: {Type} by {User}, streak={Streak}", "WatchStreak", username, streakCount);

            var media = await _twitchAlertMediaRepository.GetWatchStreakMediaAsync();
            var msg = $"👀 {username} has been watching LegendOfSacks for {streakCount} streams!";

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.TwitchWatchStreak,
                Display = $"WatchStreak: {username} ({streakCount})",
                Username = username,
                Count = streakCount,
                Message = userMessage
            });

            EnqueueAlertWithMedia(msg, media![CoreHelperMethods.GetRandomNumberForMediaSelection(media!.Count)]);

            var voice = AppSettings.Voices.WatchStreak ?? AppSettings.TTS.DefaultSpeaker ?? "Matthew";
            var template = AppSettings.Templates.Raid ?? "{raider} is storming in with {viewers} viewers!";

            var text = !string.IsNullOrWhiteSpace(userMessage)
                ? CoreHelperMethods.ForTts(userMessage)
                : CoreHelperMethods.RenderTemplate(
                    AppSettings.Templates.WatchStreak ?? "{username} has been watching Legend Of Sacks for {streak} streams!",
                    new Dictionary<string, string?>
                    {
                        ["username"] = username,
                        ["streak"] = streakCount.ToString()
                    });

            await _tsService.SpeakAsync(text, voice);
        }

        private void EnqueueAlertWithMedia(string message, string mediaPath)
        {
            if (string.IsNullOrWhiteSpace(mediaPath))
                return;

            _alertService.EnqueueAlert(message, CoreHelperMethods.ToPublicMediaPath(mediaPath));
        }

        private string GetNiceTierString(string subTier)
        {
            return subTier switch
            {
                "3000" => "3",
                "2000" => "2",
                _ => "1"
            };
        }
    }
}