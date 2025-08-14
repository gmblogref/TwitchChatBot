namespace TwitchChatBot.Core.Services.Contracts
{
    public interface ITtsService
    {
        Task SpeakAsync(string text, string? speaker = null, string? modelOverride = null);
        void SkipCurrent();   // cancel current item, move to next
        void ResetQueue();    // clear all pending, cancel current
    }
}