using System.Drawing;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IUiBridge
    {
        void AppendChat(string username, string message, Color nameColor);
        void AppendLog(string message);
        void SetViewerList(IEnumerable<string> viewers);
    }
}