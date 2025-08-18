using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IWatchStreakService
    {
        void BeginStream();
        void EndStream();

        Task MarkAttendanceAsync(string userName);
        Task MarkAttendanceBatchAsync(IEnumerable<string> userLogins);

        IReadOnlyList<WatchUserStats> TopByConsecutive(int take = 10);
        IReadOnlyList<WatchUserStats> TopByTotal(int take = 10);
        Task<(int Consecutive, int Total)> GetStatsTupleAsync(string username);
    }
}