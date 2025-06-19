using TwitchChatBot.Models;

namespace TwitchChatBot.Data.Contracts
{
    public interface ITwitchAlertMediaRepository
    {
        Task<TwitchAlertMediaMap> GetMediaMapAsync(CancellationToken cancellationToken = default);

        Task<ChannelPoints?> GetChannelPointsMapAsync(CancellationToken cancellationToken = default);
        Task<ChannelPointsText?> GetChannelPointsTextMapAsync(CancellationToken cancellationToken = default);
        Task<Cheer?> GetCheerMapAsync(CancellationToken cancellationToken = default);
        Task<List<string>?> GetFollowMediaAsync(CancellationToken cancellationToken = default);
        Task<List<string>?> GetHypeTrainMediaAsync(CancellationToken cancellationToken = default);
        Task<List<string>?> GetRaidMediaAsync(CancellationToken cancellationToken = default);
        Task<List<string>?> GetResubMediaAsync(CancellationToken cancellationToken = default);
        Task<List<string>?> GetSubgiftMediaAsync(CancellationToken cancellationToken = default);
        Task<SubMysteryGift?> GetSubMysteryGiftMapAsync(CancellationToken cancellationToken = default);
        Task<List<string>?> GetSubscriptionMediaAsync(CancellationToken cancellationToken = default);
    }
}