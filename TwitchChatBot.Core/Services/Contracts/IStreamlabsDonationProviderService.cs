namespace TwitchChatBot.Core.Services.Contracts
{
	public interface IStreamlabsDonationProviderService
	{
		Task StartAsync(CancellationToken cancellationToken = default);
		Task StopAsync(CancellationToken cancellationToken = default);
	}
}