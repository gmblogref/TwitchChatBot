using Microsoft.Extensions.Logging;
using System.Text.Json;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Data
{
    public class TwitchAlertMediaRepository : ITwitchAlertMediaRepository
    {
        private readonly ILogger<TwitchAlertMediaRepository> _logger;
        private readonly string _filePath;
        private TwitchAlertMediaMap? _cachedMediaMap;

        public TwitchAlertMediaRepository(ILogger<TwitchAlertMediaRepository> logger)
        {
            _logger = logger;
            _filePath = Path.Combine(AppContext.BaseDirectory, AppSettings.MediaFiles.TwitchAlertMedia);
        }

        public async Task<TwitchAlertMediaMap> GetMediaMapAsync(CancellationToken cancellationToken = default)
        {
            if (_cachedMediaMap != null)
                return _cachedMediaMap;

            try
            {
                if (!File.Exists(_filePath))
                    throw new FileNotFoundException("Could not find twitchAlertsMediaMap.json at path: " + _filePath);

                var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
                _cachedMediaMap = JsonSerializer.Deserialize<TwitchAlertMediaMap>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (_cachedMediaMap == null)
                    throw new InvalidOperationException("Failed to deserialize twitchAlertsMediaMap.json.");

                _logger.LogInformation("📂 TwitchAlertMediaMap loaded successfully.");
                return _cachedMediaMap;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to load TwitchAlertMediaMap.");
                throw;
            }
        }

        public async Task<ChannelPoints?> GetChannelPointsMapAsync(CancellationToken cancellationToken = default)
            => (await GetMediaMapAsync(cancellationToken)).Channel_Points;

        public async Task<ChannelPointsText?> GetChannelPointsTextMapAsync(CancellationToken cancellationToken = default)
            => (await GetMediaMapAsync(cancellationToken)).Channel_Points_Text;

        public async Task<Cheer?> GetCheerMapAsync(CancellationToken cancellationToken = default)
            => (await GetMediaMapAsync(cancellationToken)).Cheer;

        public async Task<List<string>?> GetFollowMediaAsync(CancellationToken cancellationToken = default)
            => (await GetMediaMapAsync(cancellationToken)).Follow;

        public async Task<List<string>?> GetHypeTrainMediaAsync(CancellationToken cancellationToken = default)
            => (await GetMediaMapAsync(cancellationToken)).Hype_Train;

        public async Task<List<string>?> GetRaidMediaAsync(CancellationToken cancellationToken = default)
            => (await GetMediaMapAsync(cancellationToken)).Raid;

        public async Task<List<string>?> GetResubMediaAsync(CancellationToken cancellationToken = default)
            => (await GetMediaMapAsync(cancellationToken)).Resub;

        public async Task<List<string>?> GetSubgiftMediaAsync(CancellationToken cancellationToken = default)
            => (await GetMediaMapAsync(cancellationToken)).Subgift;

        public async Task<SubMysteryGift?> GetSubMysteryGiftMapAsync(CancellationToken cancellationToken = default)
            => (await GetMediaMapAsync(cancellationToken)).Submysterygift; 
        
        public async Task<List<string>?> GetSubscriptionMediaAsync(CancellationToken cancellationToken = default)
            => (await GetMediaMapAsync(cancellationToken)).Subscription;
    }
}