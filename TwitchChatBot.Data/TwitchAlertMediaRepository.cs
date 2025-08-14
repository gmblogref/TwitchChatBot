using Microsoft.Extensions.Logging;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Data.Utilities;
using TwitchChatBot.Models;

namespace TwitchChatBot.Data
{
    public class TwitchAlertMediaRepository : ITwitchAlertMediaRepository
    {
        private readonly ILogger<TwitchAlertMediaRepository> _logger;
        private readonly string _filePath;
        private TwitchAlertMediaMap? _twitchAlertMediaMap;

        public TwitchAlertMediaRepository(ILogger<TwitchAlertMediaRepository> logger)
        {
            _logger = logger;
            _filePath = Path.Combine(AppContext.BaseDirectory, AppSettings.MediaMapFiles.TwitchAlertMedia!);
        }

        public async Task<ChannelPoints?> GetChannelPointsMapAsync(CancellationToken cancellationToken = default)
            => (await GetTwitchAlertMediaMapAsync(cancellationToken)).Channel_Points;

        public async Task<ChannelPointsText?> GetChannelPointsTextMapAsync(CancellationToken cancellationToken = default)
            => (await GetTwitchAlertMediaMapAsync(cancellationToken)).Channel_Points_Text;

        public async Task<Cheer?> GetCheerMapAsync(CancellationToken cancellationToken = default)
            => (await GetTwitchAlertMediaMapAsync(cancellationToken)).Cheer;

        public async Task<List<string>?> GetFollowMediaAsync(CancellationToken cancellationToken = default)
            => (await GetTwitchAlertMediaMapAsync(cancellationToken)).Follow;

        public async Task<List<string>?> GetHypeTrainMediaAsync(CancellationToken cancellationToken = default)
            => (await GetTwitchAlertMediaMapAsync(cancellationToken)).Hype_Train;

        public async Task<List<string>?> GetRaidMediaAsync(CancellationToken cancellationToken = default)
            => (await GetTwitchAlertMediaMapAsync(cancellationToken)).Raid;

        public async Task<List<string>?> GetResubMediaAsync(CancellationToken cancellationToken = default)
            => (await GetTwitchAlertMediaMapAsync(cancellationToken)).Resub;

        public async Task<List<string>?> GetSubgiftMediaAsync(CancellationToken cancellationToken = default)
            => (await GetTwitchAlertMediaMapAsync(cancellationToken)).Subgift;

        public async Task<SubMysteryGift?> GetSubMysteryGiftMapAsync(CancellationToken cancellationToken = default)
            => (await GetTwitchAlertMediaMapAsync(cancellationToken)).Submysterygift; 
        
        public async Task<List<string>?> GetSubscriptionMediaAsync(CancellationToken cancellationToken = default)
            => (await GetTwitchAlertMediaMapAsync(cancellationToken)).Subscription;

        private async Task<TwitchAlertMediaMap> GetTwitchAlertMediaMapAsync(CancellationToken cancellationToken = default)
        {
            if (_twitchAlertMediaMap != null)
                return _twitchAlertMediaMap;

            _twitchAlertMediaMap = await DataHelperMethods.LoadAsync<TwitchAlertMediaMap>(
                _filePath,
                _logger,
                AppSettings.MediaMapFiles.CommandAlertMedia!,
                cancellationToken
            );

            return _twitchAlertMediaMap;
        }
    }
}