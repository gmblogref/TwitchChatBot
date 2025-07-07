namespace TwitchChatBot.Core.Services.Contracts
{
    public interface ITwitchRoleService
    {
        Task<List<string>> GetModeratorsAsync(string broadcasterId);
        Task<List<string>> GetVipsAsync(string broadcasterId);
    }
}