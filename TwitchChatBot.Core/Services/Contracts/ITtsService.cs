namespace TwitchChatBot.Core.Services.Contracts
{
    public interface ITtsService
    {
        Task SpeakAsync(string text, string? speaker = null, string? modelOverride = null);
    }
}