namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IUiBridge
    {
        void AppendChat(string message);
        void AppendLog(string message);
    }
}