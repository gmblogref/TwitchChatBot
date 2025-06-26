namespace TwitchChatBot.Data.Contracts
{
    public interface IFirstChatterMediaRepository
    {
        Task<bool> IsEligibleForFirstChatAsync(string username, CancellationToken cancellationToken = default);
        Task<string?> GetFirstChatterMediaAsync(string username, CancellationToken cancellationToken = default);
    }
}