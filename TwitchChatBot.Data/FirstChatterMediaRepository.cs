using Microsoft.Extensions.Logging;
using System.Text.Json;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Data
{
    public class FirstChatterMediaRepository : IFirstChatterMediaRepository
    {
        private readonly ILogger<FirstChatterMediaRepository> _logger;
        private readonly string _filePath;
        private List<FirstChatterMediaMap>? _firstChattersMediaMap;
        private HashSet<string> _firstChatters = [];

        public FirstChatterMediaRepository(ILogger<FirstChatterMediaRepository> logger)
        {
            _logger = logger;
            _filePath = Path.Combine(AppContext.BaseDirectory, AppSettings.MediaFiles.FirstChattersMedia);
        }

        public bool HasAlreadyChatted(string username)
        {
            return _firstChatters?.Contains(username) ?? false;
        }

        public void ClearFirstChatters()
        {
            _firstChatters?.Clear();
        }

        public async Task<bool> IsEligibleForFirstChatAsync(string username)
        {
            await GetFirstChattersAsync();

            return _firstChattersMediaMap!.Exists(x => x.Username == username);
        }

        public async Task<string?> GetFirstChatterMediaAsync(string username)
        {
            await GetFirstChattersAsync();

            _firstChatters.Add(username);

            return _firstChattersMediaMap!.FirstOrDefault(x => x.Username == username)?.Media ?? null;
        }

        private async Task GetFirstChattersAsync(CancellationToken cancellationToken = default)
        {
            if (_firstChattersMediaMap != null)
                return;

            try
            {
                if (!File.Exists(_filePath))
                    throw new FileNotFoundException($"Could not find {AppSettings.MediaFiles.FirstChattersMedia} at path: {_filePath}");

                var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
                var _firstChattersMediaMap = JsonSerializer.Deserialize<List<FirstChatterMediaMap>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (_firstChattersMediaMap == null)
                    throw new InvalidOperationException($"Failed to deserialize {AppSettings.MediaFiles.FirstChattersMedia}.");

                _logger.LogInformation("📂 Excluded users loaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to load excluded users.");
                throw;
            }
        }
    }
}