using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
	public interface IDonationAlertService
	{
		Task HandleDonationAsync(DonationEvent donation);
	}
}