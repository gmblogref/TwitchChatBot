using Microsoft.Extensions.Logging;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Data.Utilities;
using TwitchChatBot.Models;

namespace TwitchChatBot.Data
{
    public class CommandMediaRepository : ICommandMediaRepository
    {
        private readonly ILogger<CommandMediaRepository> _logger;
        private readonly string _filePath;
        private CommandMediaMap? _commandMediaMap;

        public CommandMediaRepository(ILogger<CommandMediaRepository> logger)
        {
            _logger = logger;
            _filePath = Path.Combine(AppContext.BaseDirectory, AppSettings.MediaMapFiles.CommandAlertMedia);
        }

        public async Task<CommandMediaItem?> GetCommandMediaItemAsync(string command, CancellationToken cancellationToken = default)
        {
            await GetCommandsAsync(cancellationToken);

            return _commandMediaMap?.CommandMediaItems.FirstOrDefault(x => x.Command.Equals(command));
        }

        public async Task<IReadOnlyList<string>> GetAllCommandNamesAsync(CancellationToken cancellationToken = default)
        {
            await GetCommandsAsync(cancellationToken);
            return (_commandMediaMap?.CommandMediaItems ?? Enumerable.Empty<CommandMediaItem>())
                .Select(c => c.Command)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
        }

        private async Task GetCommandsAsync(CancellationToken cancellationToken = default)
        {
            if (_commandMediaMap != null && _commandMediaMap.CommandMediaItems.Count > 0)
                return;

            _commandMediaMap = await DataHelperMethods.LoadAsync<CommandMediaMap>(
                _filePath,
                _logger,
                AppSettings.MediaMapFiles.CommandAlertMedia,
                cancellationToken
            );
        }
    }
}
