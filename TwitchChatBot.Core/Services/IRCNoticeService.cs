using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Services
{
    public class IRCNoticeService : IIRCNoticeService
    {
        private readonly ILogger<IRCNoticeService> _logger;
        private readonly ITwitchAlertTypesService _twitchAlert;

        public IRCNoticeService(
            ILogger<IRCNoticeService> logger,
            ITwitchAlertTypesService twitch)
        {
            _logger = logger;
            _twitchAlert = twitch;
        }

        public async Task HandleUserNoticeAsync(IReadOnlyDictionary<string, string> tags, string? systemMsg)
        {
            if (!tags.TryGetValue("msg-id", out var msgId))
            {
                return;
            }

            // WATCH STREAK (msg-id=viewermilestone & msg-param-category=watch-streak)
            if (string.Equals(msgId, "viewermilestone", StringComparison.OrdinalIgnoreCase))
            {
                if (tags.TryGetValue("msg-param-category", out var cat))
                {
                    if (string.Equals(cat, "watch-streak", StringComparison.OrdinalIgnoreCase))
                    {
                        await HandleWatchStreak(tags, systemMsg);
                        return;
                    }
                }
            }

            // Add more USERNOTICE types here later (e.g., hype chat, community moments, goals, etc.)
        }

        private async Task HandleWatchStreak(IReadOnlyDictionary<string, string> tags, string? systemMsg)
        {
            if (!tags.TryGetValue("msg-param-value", out var valueStr))
            {
                _logger.LogDebug("USERNOTICE watch-streak missing msg-param-value. Tags: {Tags}", string.Join(",", tags.Select(kv => $"{kv.Key}={kv.Value}")));
                return;
            }

            if (!int.TryParse(valueStr, out var streak))
            {
                _logger.LogDebug("USERNOTICE watch-streak had non-numeric value: {Value}", valueStr);
                return;
            }

            var login = tags.TryGetValue("login", out var l) ? l :
                        (tags.TryGetValue("user-login", out var ul) ? ul : string.Empty);

            var display = tags.TryGetValue("display-name", out var dn) ? dn : login;
            if (string.IsNullOrWhiteSpace(display))
            {
                display = "someone";
            }

            // Some USERNOTICEs carry user-entered text in "user-message". If absent, fall back to system message.
            tags.TryGetValue("user-message", out var userMsg);
            var shared = !string.IsNullOrWhiteSpace(userMsg) ? userMsg : systemMsg;

            // Use your existing typed-alert path: server has one global queue; frontends filter on data.type
            await _twitchAlert.HandleWatchStreakNoticeAsync(display, streak, shared);

            _logger.LogInformation("🌟 Watch streak: user={User} streak={Streak}", display, streak);
        }
    }
}