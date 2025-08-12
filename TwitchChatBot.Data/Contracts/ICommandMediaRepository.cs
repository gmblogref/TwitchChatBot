using TwitchChatBot.Models;

namespace TwitchChatBot.Data.Contracts
{
    public interface ICommandMediaRepository
    {
        Task<CommandMediaItem?> GetCommandMediaItemAsync(string command, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<string>> GetAllCommandNamesAsync(CancellationToken cancellationToken = default);
    }
}
