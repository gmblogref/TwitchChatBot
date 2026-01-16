namespace TwitchChatBot.Core.Services.Contracts
{
    public interface INukeService
    {
        bool TryUseNuke(string username);
        void ClearNukes();
        void StartNukeResetTimer();
        void StopNukeResetTimer();
    }
}