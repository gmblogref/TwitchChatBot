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
        private readonly ITwitchRoleService _twitchRoleService;
        private readonly IModerationService _moderationService;
        private readonly INukeService _nukeService;
        private readonly IAppFlags _appFlags;
        
        public CommandAlertService(
            ILogger<CommandAlertService> logger,
            ICommandMediaRepository commandMediaRepository,
            IAlertService alertService,
            IExcludedUsersService excludedUsersService,
            IWatchStreakService watchStreakService,
            IHelixLookupService helixLookupService,
            ITtsService tsService,
            IAlertHistoryService alertHistoryService,
            ITwitchRoleService twitchRoleService,
            IModerationService moderationService,
            INukeService nukeService,
            IAppFlags appFlags)
        {
            _logger = logger;
            _commandMediaRepository = commandMediaRepository;
            _alertService = alertService;
            _excludedUsersService = excludedUsersService;
            _watchStreakService = watchStreakService;
            _helixLookupService = helixLookupService;
            _tsService = tsService;
            _alertHistoryService = alertHistoryService;
            _twitchRoleService = twitchRoleService;
            _moderationService = moderationService;
            _nukeService = nukeService;
            _appFlags = appFlags;
        }

        public async Task HandleCommandAsync(string commandText, string username, string channel, Action<string, string> sendMessage, bool isAutoCommand = false)
        {
            if (!isAutoCommand)
            {
                if (await _excludedUsersService.IsUserExcludedAsync(username))
                    return;
            }

            if (string.IsNullOrWhiteSpace(commandText))
                return;

            var commandTuple = ParseCommandTuple(commandText);

            var ctx = BuildContext(username, channel, commandText, commandTuple.command, commandTuple.rawTarget, commandTuple.ttsText);
            if (await ContinueIfIsSpecialCommandOptionsAsync(ctx, sendMessage))
            {
                var entry = await _commandMediaRepository.GetCommandMediaItemAsync(commandTuple.command);
                if (entry == null)
                {
                    _logger.LogDebug("❔ Unknown command: {Command}", commandTuple.command);
                    return;
                }

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
        }

        private async Task HandleTtsCommandAsync(string ttsText, string username, string commandText)
        {
            var (voice, text) = ParseTtsArgs(ttsText);
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (text.Length > 500) text = text[..500] + "…"; // simple cap

            await _tsService.SpeakAsync(text, voice, null);

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.Tts,
                Display = $"{username} ran: {commandText}",
                Username = username,
                Voice = voice,
                Message = text
            });
        }

        private sealed class CommandContext
        {
            public required string Channel { get; init; }
            public required string Username { get; init; }       // user who ran the command
            public required string Command { get; init; }
            public string? RawTarget { get; init; }             // target name without '@'
            public string? TtsText { get; init; }
            public string? CommandText { get; set; }
            public string Target => string.IsNullOrEmpty(RawTarget) ? string.Empty : $"@{RawTarget}";
            public string Url => string.IsNullOrEmpty(RawTarget) ? string.Empty : AppSettings.TwitchUrl + $"{RawTarget}";
            public string Game { get; set; } = string.Empty;    
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
            if (command is "!so" or "!raid" or "!ban" or "!nuke") // extend as needed
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

        private CommandContext BuildContext(string username, string channel, string? commandText, string command, string? rawTarget, string? ttsText)
            =>  new CommandContext() { Username = username, Channel = channel, CommandText = commandText, Command = command,  RawTarget = rawTarget, TtsText = ttsText };

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
        private async Task<bool> ContinueIfIsSpecialCommandOptionsAsync(CommandContext ctx, Action<string, string> sendMessage, CancellationToken ct = default)
        {
            switch (ctx.Command)
            {
                case "!so":
                    // Choose login to inspect: explicit target if provided, else the invoker
                    var login = string.IsNullOrEmpty(ctx.RawTarget) ? ctx.Username : ctx.RawTarget;
                    var userId = await _helixLookupService.GetUserIdByLoginAsync(login, ct);
                    if (string.IsNullOrEmpty(userId))
                        return true;

                    var game = await _helixLookupService.GetLastKnownGameByUserIdAsync(userId, ct);
                    if (!string.IsNullOrWhiteSpace(game))
                        ctx.Game = game; // token replacer will drop this into $game
                    break;
                case "!birthday":
                    await _tsService.SpeakAsync($"Happy Birthday {ctx.TtsText}");
                    break;
                case "!tts":
                    await HandleTtsCommandAsync(ctx.TtsText!, ctx.Username, ctx.CommandText!);
                    return false;
                case "!commands":
                    await HandleShowAvaiableCommands(ctx.Channel, ctx.Username, ctx.CommandText!, sendMessage);
                    return false;
                case "!nuke":
                    await HandleNukeCommandAsync(ctx, sendMessage);
                    return false;
                case "!clearnukes":
                    if(ctx.Username.ToLower() == AppSettings.TWITCH_CHANNEL!.ToLower() || ctx.Username.ToLower() == "jillybenilly")
                    {
                        _nukeService.ClearNukes();
                    }
                    return false;
            }

            return true;
        }

        private async Task HandleShowAvaiableCommands(string channel, string username, string commandText, Action<string, string> sendMessage)
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
            {
                sendMessage(channel, line);
            }

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.Cmd,
                Display = $"{username} ran: {commandText}",
                Username = username,
                CommandText = commandText
            });
        }

        private async Task HandleNukeCommandAsync(CommandContext ctx, Action<string, string> sendMessage)
        {
            // Normalize target
            if (string.IsNullOrWhiteSpace(ctx.RawTarget))
            {
                sendMessage(ctx.Channel, "Usage: !nuke targetUser");
                return;
            }

            var targetLogin = ctx.RawTarget.Trim().TrimStart('@');
            if (string.IsNullOrWhiteSpace(targetLogin))
            {
                sendMessage(ctx.Channel, "Usage: !nuke targetUser");
                return;
            }

            // One-use-per-stream rule
            if (!_appFlags.IsReplay && !_nukeService.TryUseNuke(ctx.Username))
            {
                sendMessage(ctx.Channel, $"@{ctx.Username}, you've already used your one nuke this stream.");
                return;
            }
            
            // Optional: mod safety check if your GetModeratorsAsync returns logins
            var mods = await _twitchRoleService.GetModeratorsAsync(AppSettings.TWITCH_USER_ID!);
            var isTargetBroadcaster = targetLogin.Equals(AppSettings.TWITCH_CHANNEL, StringComparison.OrdinalIgnoreCase);
            var isTargetBot = targetLogin.Equals(AppSettings.TWITCH_BOT_USERNAME, StringComparison.OrdinalIgnoreCase);
            var isTargetMod = !isTargetBot && mods.Contains(targetLogin, StringComparer.OrdinalIgnoreCase);
            var useBot = (!isTargetBot && !isTargetMod);

            try
            {
                // Resolve target user_id
                var targetId = await _moderationService.GetUserIdAsync(targetLogin);

                if (isTargetMod)
                {
                    // Helix timeout (10s example)
                    await _moderationService.TimeoutAsync(AppSettings.TWITCH_USER_ID!, AppSettings.TWITCH_USER_ID!, targetId, 5, useBot);

                    // Fun line + media for mod nukes
                    sendMessage(ctx.Channel, $"⚠️ @{ctx.Username} is attacking the MODs… let’s see how that works out. 🔥");

                    var entry = await _commandMediaRepository.GetCommandMediaItemAsync("!nukeMod");
                    _alertService.EnqueueAlert("", CoreHelperMethods.ToPublicMediaPath(entry!.Media!));
                }
                else if (isTargetBot)
                {
                    // Helix timeout (10s example)
                    await _moderationService.TimeoutAsync(AppSettings.TWITCH_USER_ID!, AppSettings.TWITCH_USER_ID!, targetId, 5, useBot);

                    // TTS reminder to re-mod the bot afterward
                    sendMessage(ctx.Channel, $"BOOM! 💣 @{ctx.Username} nuked the bot!");
                    _tsService?.SpeakAsync("Hey, LegendOfSacks, you need to mod the bot again.");
                }
                else if (isTargetBroadcaster)
                {
                    var entry = await _commandMediaRepository.GetCommandMediaItemAsync("!nukeSacks");
                    _alertService.EnqueueAlert("fullscreen", "", CoreHelperMethods.ToPublicMediaPath(entry!.Media!));
                }
                else
                {
                    // Helix timeout (10s example)
                    await _moderationService.TimeoutAsync(AppSettings.TWITCH_USER_ID!, AppSettings.TWITCH_BOT_ID!, targetId, 5, useBot);

                    // Announce success
                    sendMessage(ctx.Channel, $"💣 {targetLogin} bye bye!!!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NUKE Helix path failed for target {TargetLogin}", targetLogin);
            }

            _alertHistoryService.Add(new AlertHistoryEntry
            {
                Type = AlertHistoryType.Cmd,
                Display = $"{ctx.Username} nuked {ctx.RawTarget}",
                Username = ctx.Username,
                CommandText = ctx.CommandText!,
                Message = $"{ctx.RawTarget} nuked by {ctx.Username}"
            });
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