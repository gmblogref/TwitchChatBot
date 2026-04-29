using TwitchChatBot.Models;

namespace TwitchChatBot.Data.Contracts
{
	public interface IDonationMediaRepository
	{
		Task<DonationMediaMap?> GetDonationMapAsync(
			CancellationToken cancellationToken = default);
	}
}