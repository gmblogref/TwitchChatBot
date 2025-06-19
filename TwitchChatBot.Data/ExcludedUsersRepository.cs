using Microsoft.Extensions.Logging;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Data.Utilities;
using TwitchChatBot.Models;

namespace TwitchChatBot.Data
{
    public class ExcludedUsersRepository : IExcludedUsersRepository
    {
        private readonly ILogger<ExcludedUsersRepository> _logger;
        private readonly string _filePath;
        private HashSet<string>? _excludedUsers;

        public ExcludedUsersRepository(ILogger<ExcludedUsersRepository> logger)
        {
            _logger = logger;
            _filePath = Path.Combine(AppContext.BaseDirectory, AppSettings.MediaFiles.ExcludedUsersMedia);
        }

        public async Task<bool> IsUserExcludedAsync(string username, CancellationToken cancellationToken = default)
        {
            await GetExcludedUsersAsync(cancellationToken);
            return _excludedUsers!.Contains(username.ToLowerInvariant());
        }
        
        private async Task GetExcludedUsersAsync(CancellationToken cancellationToken = default)
        {
            if (_excludedUsers != null)
                return;

            _excludedUsers = await DataHelperMethods.LoadAsync<HashSet<string>>(
                _filePath,
                _logger,
                AppSettings.MediaFiles.ExcludedUsersMedia,
                cancellationToken
            );
        }
    }
}