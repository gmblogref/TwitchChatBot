namespace TwitchChatBot.Core.Services.Contracts
{
    public interface ICommandAlertService
    {
        Task HandleCommandAsync(string commandText, string username, string channel, Action<string, string> sendMessage);
    }
}