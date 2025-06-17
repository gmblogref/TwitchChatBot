namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IEventSubService
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}