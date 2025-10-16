using System.Collections.Generic;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IIRCNoticeService
    {
        void HandleUserNotice(IReadOnlyDictionary<string, string> tags, string? systemMsg);
    }
}