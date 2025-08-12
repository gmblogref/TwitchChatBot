using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Core.Utilities;
using TwitchChatBot.Data.Contracts;

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

        public CommandAlertService(
            ILogger<CommandAlertService> logger,
            ICommandMediaRepository commandMediaRepository,
            IAlertService alertService,
            IExcludedUsersService excludedUsersService,
            IWatchStreakService watchStreakService,
            IHelixLookupService helixLookupService)
        {
            _logger = logger;
            _commandMediaRepository = commandMediaRepository;
            _alertService = alertService;
            _excludedUsersService = excludedUsersService;
            _watchStreakService = watchStreakService;
            _helixLookupService = helixLookupService;
        }

        public async Task HandleCommandAsync(string commandText, string username, string channel, Action<string, string> sendMessage)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                return;

            var commandTuple = ParseCommandTuple(commandText);

            if (commandTuple.command == "!commands")
            {
                await HandleShowAvaiableCommands(channel, sendMessage);
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
                    sendMessage(channel, message);
                }
            }

            // MEDIA Command
            if (!string.IsNullOrWhiteSpace(entry.Media))
            {
                _alertService.EnqueueAlert("", CoreHelperMethods.ToPublicMediaPath(entry.Media));
            }
        }

        private sealed class CommandContext
        {
            public required string Channel { get; init; }
            public required string Username { get; init; }       // user who ran the command
            public required string RawTarget { get; init; }      // target name without '@'
            public string Target => string.IsNullOrEmpty(RawTarget) ? string.Empty : $"@{RawTarget}";
            public string Url => string.IsNullOrEmpty(RawTarget) ? string.Empty : $"https://twitch.tv/{RawTarget}";
            public string Game { get; set; } = string.Empty;    // TODO: fill via Helix later
        }

        /// <summary>
        /// Gets ValueTuple of Command text and the Target text
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        private (string command, string rawTarget) ParseCommandTuple(string commandText)
        {
            var parts = commandText.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLowerInvariant();
            var rawTarget = parts.Length > 1 ? parts[1].TrimStart('@') : string.Empty;
            return (command, rawTarget);
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
                .Replace("$url", string.IsNullOrEmpty(ctx.RawTarget) ? $"https://twitch.tv/{ctx.Username}" : ctx.Url, StringComparison.OrdinalIgnoreCase)
                .Replace("$game", ctx.Game, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Supplies optional string.Format args per command. For most commands, return null/empty.
        /// </summary>
        private async Task<object[]?> GetFormatArgsForCommandAsync(string command, CommandContext ctx)
        {
            switch (command)
            {
                case "!streak":
                    // Optional: block excluded users
                    if (await _excludedUsersService.IsUserExcludedAsync(ctx.Username))
                        return Array.Empty<object>(); // no args -> message stays as-is (safe no-op)

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