namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IWebHostWrapper
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
