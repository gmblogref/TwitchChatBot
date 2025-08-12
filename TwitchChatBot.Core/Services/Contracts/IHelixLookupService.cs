namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IHelixLookupService
    {
        
        Task<string?> GetUserIdByLoginAsync(string login, CancellationToken ct = default);
        Task<string?> GetLastKnownGameByUserIdAsync(string userId, CancellationToken ct = default);

        Task<IReadOnlyList<string>> GetModeratorLoginsAsync(string broadcasterId, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetVipLoginsAsync(string broadcasterId, CancellationToken ct = default);
    }
}