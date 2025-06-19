namespace TwitchChatBot.Data.Contracts
{
    public interface IFirstChatterMediaRepository
    {
        void ClearFirstChatters();
        bool HasAlreadyChatted(string username);
        void MarkAsChatted(string username);
        Task<bool> IsEligibleForFirstChatAsync(string username, CancellationToken cancellationToken = default);
        Task<string?> GetFirstChatterMediaAsync(string username, CancellationToken cancellationToken = default);
    }
}