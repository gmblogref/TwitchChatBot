using TwitchChatBot.Models;

namespace TwitchChatBot.Data.Contracts
{
    public interface IFirstChatterMediaRepository
    {
        Task<FirstChatterMediaItem?> GetFirstChatterMediaAsync(
        string userId,
        string username,
        CancellationToken cancellationToken = default);

        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}