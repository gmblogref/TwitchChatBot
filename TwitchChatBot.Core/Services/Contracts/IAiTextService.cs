using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IAiTextService
    {
        Task<string?> GenerateAlertLineAsync(AlertContext context);
    }
}