using Microsoft.Extensions.Logging;
using System.Text.Json;
using TwitchChatBot.Data.Contracts;
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
            var users = await GetExcludedUsersAsync(cancellationToken);
            return users.Contains(username.ToLowerInvariant());
        }
        
        private async Task<HashSet<string>> GetExcludedUsersAsync(CancellationToken cancellationToken = default)
        {
            if (_excludedUsers != null)
                return _excludedUsers;

            try
            {
                if (!File.Exists(_filePath))
                    throw new FileNotFoundException("Could not find excludedUsers.json at path: " + _filePath);

                var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
                var list = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (list == null)
                    throw new InvalidOperationException("Failed to deserialize excludedUsers.json.");

                _excludedUsers = new HashSet<string>(list.Select(name => name.ToLowerInvariant()));
                _logger.LogInformation("📂 Excluded users loaded successfully.");
                return _excludedUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to load excluded users.");
                throw;
            }
        }
    }
}