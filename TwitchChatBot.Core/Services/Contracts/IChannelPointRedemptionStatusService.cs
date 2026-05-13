namespace TwitchChatBot.Core.Services.Contracts
{
	public interface IChannelPointRedemptionStatusService
	{
		Task<bool> TryCancelRedemptionAsync(
			string redemptionId,
			string rewardId,
			CancellationToken cancellationToken = default);
	}
}