using TwitchChatBot.Models;

namespace TwitchChatBot.Data.Contracts
{
	public interface IWheelRepository
	{
		Task<List<Wheel>> GetAllAsync(CancellationToken cancellationToken = default);

		Task SaveAllAsync(List<Wheel> wheels, CancellationToken cancellationToken = default);
	}
}