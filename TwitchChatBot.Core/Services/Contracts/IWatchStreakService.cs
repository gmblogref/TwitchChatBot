using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IWatchStreakService
    {
        Task BeginStreamAsync();
        Task EndStreamAsync();

        Task MarkAttendanceAsync(string userId, string userName);

        Task<(int Consecutive, int Total)> GetStatsTupleAsync(string userId, string username);
    }
}