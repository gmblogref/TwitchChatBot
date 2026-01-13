using TwitchChatBot.Models;

namespace TwitchChatBot.Data.Contracts
{
    public interface IWatchStreakRepository
    {
        Task<WatchStreakState> GetStateAsync(CancellationToken cancellationToken = default);
        Task SaveAsync(WatchStreakState state, CancellationToken cancellationToken = default);
    }
}