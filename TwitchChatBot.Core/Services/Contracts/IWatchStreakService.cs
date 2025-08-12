using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
    public enum StreamSessionMode { Off = 0, Testing = 1, Live = 2 }

    public interface IWatchStreakService
    {
        StreamSessionMode Mode { get; }
        bool IsLive { get; }

        void BeginStream(StreamSessionMode mode);
        void EndStream();

        Task MarkAttendanceAsync(string userName);
        Task MarkAttendanceBatchAsync(IEnumerable<string> userLogins);

        IReadOnlyList<WatchUserStats> TopByConsecutive(int take = 10);
        IReadOnlyList<WatchUserStats> TopByTotal(int take = 10);
        Task<(int Consecutive, int Total)> GetStatsTupleAsync(string username);
    }
}