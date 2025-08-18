using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Core.Utilities;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class CommandAlertService : ICommandAlertService
    {
        private readonly ILogger<CommandAlertService> _logger;
        private readonly ICommandMediaRepository _commandMediaRepository;
        private readonly IAlertService _alertService;
        private readonly IExcludedUsersService _excludedUsersService;
        private readonly IWatchStreakService _watchStreakService;
        private readonly IHelixLookupService _helixLookupService;
        private readonly ITtsService _tsService;
        private readonly IAlertHistoryService _alertHistoryService;

        public CommandAlertService(
            ILogger<CommandAlertService> logger,
            ICommandMediaRepository commandMediaRepository,
            IAlertService alertService,
            IExcludedUsersService excludedUsersService,
            IWatchStreakService watchStreakService,
            IHelixLookupService helixLookupService,
            ITtsService tsService,
            IAlertHistoryService alertHistoryService)
        {
            _logger = logger;
            _commandMediaRepository = commandMediaRepository;
            _alertService = alertService;
            _excludedUsersService = excludedUsersService;
            _watchStreakService = watchStreakService;
            _helixLookupService = helixLookupService;
            _tsService = tsService;
            _alertHistoryService = alertHistoryService;
        }

        public async Task HandleCommandAsync(string commandText, string username, string channel, Action<string, string> sendMessage)
        {
            if (await _excludedUsersService.IsUserExcludedAsync(username))
                return;

            if (string.IsNullOrWhiteSpace(commandText))
                return;

            var commandTuple = ParseCommandTuple(commandText);

            if (commandTuple.command == "!commands")
            {
                await HandleShowAvaiableCommands(channel, sendMessage);

                _alertHistoryService.Add(new AlertHistoryEntry
                {
                    Type = AlertHistoryType.Cmd,
                    Display = $"{username} ran: {commandText}",
                    Username = username,
                    CommandText = commandText
                });

                return;
            }

            // ---- special: !tts ----
            if (commandTuple.command.ToLower() == "!tts")
            {
                var (voice, text) = ParseTtsArgs(commandTuple.ttsText);
                if (string.IsNullOrWhiteSpace(text))
                {
                    return;
                }

                if (text.Length > 500) text = text[..500] + "…"; // simple cap

                await _tsService.SpeakAsync(text, voice, null);

                _alertHistoryService.Add(new AlertHistoryEntry
                {
                    Type= AlertHistoryType.Tts,
                    Display = $"{username} ran: {commandText}",
                    Username = username,
                    Voice = voice,
                    Message = text
                });

                return;
            }

            var entry = await _commandMediaRepository.GetCommandMediaItemAsync(commandTuple.command);
            if (entry == null)
            {
                _logger.LogDebug("❔ Unknown command: {Command}", commandTuple.command);
                return;
            }

            var ctx = BuildContext(username, channel, commandTuple.rawTarget);
            await DoSpecialCommandOptions(commandTuple.command, ctx);

            // TEXT Command
            if (!string.IsNullOrWhiteSpace(entry.Text))
            {
                var templated = ReplaceTokens(entry.Text, ctx);

                // Supply per-command formatting args (e.g., !streak → {0} {1})
                var formatArgs = await GetFormatArgsForCommandAsync(commandTuple.command, ctx);

                var message = SafeFormat(templated, formatArgs);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    _alertHistoryService.Add(new AlertHistoryEntry
                    {
                        Type = AlertHistoryType.Cmd,
                        Display = $"{username} ran: {commandText}",
                        Username = username,
                        CommandText = commandText,
                        Message = message
                    });

                    sendMessage(channel, message);
                }
            }

            // MEDIA Command
            if (!string.IsNullOrWhiteSpace(entry.Media))
            {
                _alertHistoryService.Add(new AlertHistoryEntry
                {
                    Type = AlertHistoryType.Cmd,
                    Display = $"{username} ran: {commandText}",
                    Username = username,
                    CommandText = commandText,
                    MediaPath = entry.Media
                });

                _alertService.EnqueueAlert("", CoreHelperMethods.ToPublicMediaPath(entry.Media));
            }
        }

        private sealed class CommandContext
        {
            public required string Channel { get; init; }
            public required string Username { get; init; }       // user who ran the command
            public required string RawTarget { get; init; }      // target name without '@'
            public string Target => string.IsNullOrEmpty(RawTarget) ? string.Empty : $"@{RawTarget}";
            public string Url => string.IsNullOrEmpty(RawTarget) ? string.Empty : AppSettings.TwitchUrl + $"{RawTarget}";
            public string Game { get; set; } = string.Empty;    // TODO: fill via Helix later
        }

        /// <summary>
        /// Gets ValueTuple of Command text and the Target text
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        private (string command, string rawTarget, string ttsText) ParseCommandTuple(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                return (string.Empty, string.Empty, string.Empty);
            }

            // Split only once: [command] [remainder...]
            var firstSplit = commandText.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var command = firstSplit[0].ToLowerInvariant();
            var remainder = firstSplit.Length > 1 ? firstSplit[1] : string.Empty;

            // Only commands that actually need a target should parse one out here.
            // Example: !so @user (or !so user)
            string rawTarget = string.Empty;
            if (command is "!so" or "!raid" or "!ban" or "!timeout") // extend as needed
            {
                var parts = remainder.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                rawTarget = parts.Length > 0 ? parts[0].TrimStart('@') : string.Empty;
                remainder = parts.Length > 1 ? parts[1] : string.Empty; // remainder after the target
            }

            return (command, rawTarget, remainder);
        }

        /// <summary>
        /// Gets ValueTuple of TTS voice and TTS text to speak
        /// </summary>
        /// <param name="remainder"></param>
        /// <returns></returns>
        private (string? voice, string text) ParseTtsArgs(string remainder)
        {
            if (string.IsNullOrWhiteSpace(remainder))
            {
                return (null, string.Empty);
            }

            // [maybeVoice] [rest...]
            var parts = remainder.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                return (null, parts[0]);
            }

            var maybeVoice = parts[0];
            var rest = parts[1];

            // Not a recognized voice → treat full remainder as text
            return (maybeVoice, rest);
        }

        private CommandContext BuildContext(string username, string channel, string rawTarget)
            => new CommandContext { Username = username, Channel = channel, RawTarget = rawTarget };

        /// <summary>
        /// Replaces your existing tokens. Keeps behavior consistent with !so.
        /// Supports: @$targetname, $target, $url, $game
        /// </summary>
        private string ReplaceTokens(string template, CommandContext ctx)
        {
            // @$targetname first so the '@' stays intact
            return template
                .Replace("@$targetname", $"@{(string.IsNullOrEmpty(ctx.RawTarget) ? ctx.Username : ctx.RawTarget)}", StringComparison.OrdinalIgnoreCase)
                .Replace("$targetname", (string.IsNullOrEmpty(ctx.RawTarget) ? ctx.Username : ctx.RawTarget), StringComparison.OrdinalIgnoreCase)
                .Replace("$target", string.IsNullOrEmpty(ctx.RawTarget) ? $"@{ctx.Username}" : ctx.Target, StringComparison.OrdinalIgnoreCase)
                .Replace("$url", string.IsNullOrEmpty(ctx.RawTarget) ? AppSettings.TwitchUrl + $"{ctx.Username}" : ctx.Url, StringComparison.OrdinalIgnoreCase)
                .Replace("$game", ctx.Game, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Supplies optional string format args per command. For most commands, return null/empty.
        /// </summary>
        private async Task<object[]?> GetFormatArgsForCommandAsync(string command, CommandContext ctx)
        {
            switch (command)
            {
                case "!streak":
                    var statsTuple = await _watchStreakService.GetStatsTupleAsync(ctx.Username);
                    return new object[] { statsTuple.Consecutive, statsTuple.Total };

                default:
                    return null; // no formatting placeholders expected
            }
        }

        /// <summary>
        /// Is the command a special command that is not in the JSON file
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private async Task DoSpecialCommandOptions(string command, CommandContext ctx, CancellationToken ct = default)
        {
            switch (command)
            {
                case "!so":
                    // Choose login to inspect: explicit target if provided, else the invoker
                    var login = string.IsNullOrEmpty(ctx.RawTarget) ? ctx.Username : ctx.RawTarget;
                    var userId = await _helixLookupService.GetUserIdByLoginAsync(login, ct);
                    if (string.IsNullOrEmpty(userId))
                        return;

                    var game = await _helixLookupService.GetLastKnownGameByUserIdAsync(userId, ct);
                    if (!string.IsNullOrWhiteSpace(game))
                        ctx.Game = game; // token replacer will drop this into $game
                    break;
            }
        }

        private async Task HandleShowAvaiableCommands(string channel, Action<string, string> sendMessage)
        {
            var names = await _commandMediaRepository.GetAllCommandNamesAsync();
            if (names.Count == 0)
            {
                sendMessage(channel, "No commands configured.");
                return;
            }

            // Twitch chat length safety (~500 chars). Send in chunks if needed.
            const int maxLen = 450;
            var prefix = "Available commands: ";
            var line = prefix;

            foreach (var name in names)
            {
                var next = (line.Length == prefix.Length) ? name : ", " + name;
                if ((line.Length + next.Length) > maxLen)
                {
                    sendMessage(channel, line);
                    line = prefix + name;
                }
                else
                {
                    line += next;
                }
            }
            if (line.Length > prefix.Length)
                sendMessage(channel, line);
        }

        /// <summary>
        /// Formats with args if provided; if placeholders/args mismatch, logs and falls back to template.
        /// </summary>
        private string SafeFormat(string template, object[]? args)
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            if (args == null || args.Length == 0)
                return template; // no numeric placeholders expected

            try
            {
                return string.Format(template, args);
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Command template format mismatch. Template: {Template}", template);
                return template; // fallback so the bot still replies
            }
        }
    }
}