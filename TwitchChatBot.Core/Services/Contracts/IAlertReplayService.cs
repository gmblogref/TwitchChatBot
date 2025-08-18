using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IAlertReplayService
    {
        Task ReplayAsync(AlertHistoryEntry entry, CancellationToken ct = default);
    }
}
