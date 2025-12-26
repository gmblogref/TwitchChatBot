namespace TwitchChatBot.Core.Services.Contracts
{
    public interface ICommandAlertService
    {
        Task HandleCommandAsync(string commandText, string username, string channel, Action<string, string> sendMessage, bool isAutoCommand = false);

        Task TryAutoShoutOutIfStreamerAsync(string username, string channel, Action<string, string> sendMessage, CancellationToken ct = default);
    }
}