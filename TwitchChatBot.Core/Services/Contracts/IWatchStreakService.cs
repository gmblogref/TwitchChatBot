using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IWatchStreakService
    {
        void BeginStream();
        void EndStream();

        Task MarkAttendanceAsync(string userId, string userName);

        Task<(int Consecutive, int Total)> GetStatsTupleAsync(string userId, string username);
    }
}