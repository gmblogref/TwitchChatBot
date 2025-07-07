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
        
        public CommandAlertService(
            ILogger<CommandAlertService> logger,
            ICommandMediaRepository commandMediaRepository,
            IAlertService alertService)
        {
            _logger = logger;
            _commandMediaRepository = commandMediaRepository;
            _alertService = alertService;
        }

        public async Task HandleCommandAsync(string commandText, string username, string channel, Action<string, string> sendMessage)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                return;

            var parts = commandText.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLower();

            var entry = await _commandMediaRepository.GetCommandMediaItemAsync(command);
            if (entry == null)
            {
                _logger.LogDebug("❔ Unknown command: {Command}", command);
                return;
            }

            // Parse optional target for token replacement
            string rawTarget = parts.Length > 1 ? parts[1].TrimStart('@') : string.Empty;
            string target = $"@{rawTarget}";
            string url = !string.IsNullOrEmpty(rawTarget) ? $"https://twitch.tv/{rawTarget}" : "";
            string game = string.Empty; // TODO: Replace with live Twitch API later

            // Replace tokens if text is present
            if (!string.IsNullOrWhiteSpace(entry.Text))
            {
                var formatted = entry.Text
                    .Replace("$target", target)
                    .Replace("$targetname", rawTarget)
                    .Replace("$url", url)
                    .Replace("$game", game);

                sendMessage(channel, formatted);
            }

            // Play media if exists
            if (!string.IsNullOrWhiteSpace(entry.Media))
            {
                _alertService.EnqueueAlert("", CoreHelperMethods.ToPublicMediaPath(entry.Media));
            }
        }
    }
}