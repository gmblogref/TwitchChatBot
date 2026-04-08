using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
	public interface IWheelService
	{
		Task<List<Wheel>> GetAllWheelsAsync();

		Task<Wheel?> GetWheelAsync(string wheelId);

		Task AddWheelAsync(Wheel wheel);

		Task AddItemAsync(string wheelId, WheelItem item);

		Task RemoveItemAsync(string wheelId, string itemId);

		Task ToggleHiddenAsync(string wheelId, string itemId);

		Task ShuffleAsync(string wheelId);

		Task<WheelItem?> SpinAsync(string wheelId);

		Task ExecuteAsync(WheelItem item);
	}
}