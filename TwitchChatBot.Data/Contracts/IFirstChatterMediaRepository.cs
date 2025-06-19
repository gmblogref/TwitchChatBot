namespace TwitchChatBot.Data.Contracts
{
    public interface IFirstChatterMediaRepository
    {
        void ClearFirstChatters();
        bool HasAlreadyChatted(string username);
        Task<bool> IsEligibleForFirstChatAsync(string username);
        Task<string?> GetFirstChatterMediaAsync(string username);
    }
}