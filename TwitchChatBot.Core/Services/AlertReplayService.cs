using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class AlertReplayService : IAlertReplayService
    {
        private readonly ILogger<AlertReplayService> _logger;
        private readonly IAlertService _alertService;
        private readonly IFirstChatterAlertService _firstChatter;
        private readonly ICommandAlertService _command;
        private readonly ITwitchAlertTypesService _twitch;
        private readonly ITtsService _tts;
        private readonly ITwitchClientWrapper _twitchClient; // for sendMessage delegate
        private readonly IAppFlags _appFlags;

        public AlertReplayService(
            ILogger<AlertReplayService> logger,
            IAlertService alertService,
            IFirstChatterAlertService firstChatterAlertService,
            ICommandAlertService commandAlertService,
            ITwitchAlertTypesService twitchAlertTypesService,
            ITtsService ttsService,
            ITwitchClientWrapper twitchClient,
            IAppFlags appFlags)
        {
            _logger = logger;
            _alertService = alertService;
            _firstChatter = firstChatterAlertService;
            _command = commandAlertService;
            _twitch = twitchAlertTypesService;
            _tts = ttsService;
            _twitchClient = twitchClient;
            _appFlags = appFlags;
        }

        public async Task ReplayAsync(AlertHistoryEntry e, CancellationToken ct = default)
        {
            using (_appFlags.BeginReplayScope())
            {
                // Normalize tier and viewers aliases
                var tier = e.Tier;
                var viewers = e.Viewers ?? e.Count;

                _logger.LogInformation("🔁 Replaying alert: {Type} {User} {Display}", e.Type, e.Username, e.Display);

                switch (e.Type)
                {
                    case AlertHistoryType.First:
                        if (!string.IsNullOrWhiteSpace(e.Username))
                            await _firstChatter.HandleFirstChatAsync(e.Username, e.Display, true);
                        else
                            _logger.LogWarning("Replay First: missing Username.");
                        break;

                    case AlertHistoryType.Cmd:
                        if (!string.IsNullOrWhiteSpace(e.CommandText))
                            await _command.HandleCommandAsync(e.CommandText, e.Username ?? "someone", AppSettings.TWITCH_CHANNEL!, _twitchClient.SendMessage);
                        else
                            _logger.LogWarning("Replay Cmd: missing CommandText.");
                        break;

                    case AlertHistoryType.Tts:
                        // Prefer to regenerate via TTS if we have text; else fall back to media path
                        if (!string.IsNullOrWhiteSpace(e.Message))
                            await _tts.SpeakAsync(e.Message!, e.Voice, null);
                        else if (!string.IsNullOrWhiteSpace(e.MediaPath))
                            _alertService.EnqueueAlert("TTS", e.MediaPath!);
                        else
                            _logger.LogWarning("Replay TTS: missing Message/MediaPath.");
                        break;

                    case AlertHistoryType.TwitchFollow:
                        if (!string.IsNullOrWhiteSpace(e.Username))
                            await _twitch.HandleFollowAsync(e.Username!);
                        else
                            _logger.LogWarning("Replay Follow: missing Username.");
                        break;

                    case AlertHistoryType.TwitchRaid:
                        if (!string.IsNullOrWhiteSpace(e.Username) && viewers.HasValue)
                            await _twitch.HandleRaidAsync(e.Username!, viewers!.Value);
                        else
                            _logger.LogWarning("Replay Raid: missing Username/Viewers.");
                        break;

                    case AlertHistoryType.TwitchCheer:
                        await _twitch.HandleCheerAsync(
                            e.Username ?? "someone",
                            e.Bits ?? 0,
                            e.Message ?? "");
                        break;

                    case AlertHistoryType.TwitchSub:
                        // first-time sub; tier may be null (we’ll pass empty)
                        await _twitch.HandleSubscriptionAsync(
                            e.Username ?? "someone",
                            tier ?? "");
                        break;

                    case AlertHistoryType.TwitchSubMessage:
                        await _twitch.HandleResubAsync(
                            e.Username ?? "someone",
                            e.Months ?? 0,
                            e.Message ?? string.Empty,
                            tier ?? "");
                        break;

                    case AlertHistoryType.TwitchSubGift:
                        await _twitch.HandleSubGiftAsync(
                            e.Gifter ?? (e.Username ?? "someone"),
                            e.Recipient ?? "someone",
                            tier ?? "");
                        break;

                    case AlertHistoryType.TwitchMysteryGift:
                        await _twitch.HandleSubMysteryGiftAsync(
                            e.Gifter ?? "Anonymous",
                            e.Count ?? 0,
                            tier ?? "");
                        break;

                    case AlertHistoryType.ChannelPoint:
                        // Recreate a channel-point alert; you may have a HandleChannelPointAsync
                        await _twitch.HandleChannelPointRedemptionAsync(
                            e.Username ?? "someone",
                            e.RewardTitle ?? "Channel Point");
                        break;

                    default:
                        // Fallback: if we at least have media, replay via media queue
                        if (!string.IsNullOrWhiteSpace(e.MediaPath))
                            _alertService.EnqueueAlert(e.Message ?? "Alert", e.MediaPath!);
                        else
                            _logger.LogWarning("Replay default: Unknown type {Type} and no MediaPath.", e.Type);
                        break;
                }
            }
        }
    }
}