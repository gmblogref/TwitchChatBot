using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services.Contracts
{
	public interface IWheelService
	{
		Task<List<Wheel>> GetAllWheelsAsync();

		Task<Wheel?> GetWheelAsync(string wheelId);

		Task<bool> AddWheelAsync(Wheel wheel);

		Task AddItemAsync(string wheelId, WheelItem item);

		Task RemoveItemAsync(string wheelId, string itemId);

		Task ToggleHiddenAsync(string wheelId, string itemId);

		Task ShuffleAsync(string wheelId);

		Task<WheelItem?> SpinAsync(string wheelId);

		Task TriggerWheelAlertAsync(WheelItem item);

		Task<bool> RenameWheelAsync(string wheelId, string newName);

		Task<bool> DeleteWheelAsync(string wheelId);

		Task UpdateItemAsync(string wheelId, WheelItem item);
	}
}