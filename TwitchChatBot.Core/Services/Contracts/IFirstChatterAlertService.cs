using System.Threading.Tasks;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IFirstChatterAlertService
    {
        void ClearFirstChatters();
        Task<bool> HandleFirstChatAsync(string username, string displayName, bool isReplay = false);
    }
}