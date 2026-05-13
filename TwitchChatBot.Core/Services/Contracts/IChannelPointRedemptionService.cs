using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
	public interface IChannelPointRedemptionService
	{
		Task HandleRedemptionAsync(ChannelPointRedemptionEvent redemption);
	}
}