using Microsoft.Extensions.Logging;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Data.Utilities;
using TwitchChatBot.Models;

namespace TwitchChatBot.Data
{
    public class FirstChatterMediaRepository : IFirstChatterMediaRepository
    {
        private readonly ILogger<FirstChatterMediaRepository> _logger;
        private readonly string _filePath;
        private FirstChatterMediaMap? _firstChattersMediaMap;

        public FirstChatterMediaRepository(ILogger<FirstChatterMediaRepository> logger)
        {
            _logger = logger;
            _filePath = DataHelperMethods.GetFirstChattersMediaPath();
        }

        public async Task<bool> IsEligibleForFirstChatAsync(string username, CancellationToken cancellationToken = default)
        {
            await GetFirstChattersAsync(cancellationToken);

            return _firstChattersMediaMap!.FirstChatterMediaItems.Exists(x => x.Username == username);
        }

        public async Task<string?> GetFirstChatterMediaAsync(string username, CancellationToken cancellationToken = default)
        {
            await GetFirstChattersAsync(cancellationToken);

            return _firstChattersMediaMap!.FirstChatterMediaItems.FirstOrDefault(x => x.Username == username)?.Media ?? null;
        }

        private async Task GetFirstChattersAsync(CancellationToken cancellationToken = default)
        {
            if (_firstChattersMediaMap != null && _firstChattersMediaMap.FirstChatterMediaItems.Count > 0)
                return;

            _firstChattersMediaMap = await DataHelperMethods.LoadAsync<FirstChatterMediaMap>(
                _filePath,
                _logger,
                AppSettings.MediaMapFiles.FirstChattersMedia!,
                cancellationToken);
        }
    }
}