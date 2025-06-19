namespace TwitchChatBot.Data.Contracts
{
    public interface IExcludedUsersRepository
    {
        Task<bool> IsUserExcludedAsync(string username, CancellationToken cancellationToken = default);
    }
}