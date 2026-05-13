using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
	public interface IChannelPointRedemptionLockService
	{
		ChannelPointLockResult TryUseRewardGroup(string userId, string userName, string rewardTitle);
		void Clear();
	}
}