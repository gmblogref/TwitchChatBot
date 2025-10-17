namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IIRCNoticeService
    {
        Task HandleUserNoticeAsync(IReadOnlyDictionary<string, string> tags, string? systemMsg);
    }
}