using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IAlertHistoryService
    {
        void Add(AlertHistoryEntry entry);
        IReadOnlyList<AlertHistoryEntry> Snapshot();
        void Clear();

        // Event for UI to subscribe and update the list live
        event Action<AlertHistoryEntry>? EntryAdded;
    }
}