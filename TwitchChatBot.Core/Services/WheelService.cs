using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Providers;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
	public class WheelService : IWheelService
	{
		private readonly IWheelRepository _wheelRepository;
		private readonly ITwitchClientWrapper _twitchClientWrapper;
		private readonly ITwitchAlertTypesService _twitchAlertTypesService;
		private readonly ICommandAlertService _commandAlertService;
		private readonly IAlertService _alertService;
		private readonly IRandomProvider _randomProvider;
		private readonly ILogger<WheelService> _logger;

		private readonly object _spinLock = new();
		private bool _isSpinning;
		
		public WheelService(
			IWheelRepository wheelRepository,
			ITwitchClientWrapper twitchClientWrapper,
			ITwitchAlertTypesService twitchAlertTypesService,
			ICommandAlertService commandAlertService,
			IAlertService alertService,
			IRandomProvider randomProvider,
			ILogger<WheelService> logger)

		{
			_wheelRepository = wheelRepository;
			_twitchClientWrapper = twitchClientWrapper;
			_twitchAlertTypesService = twitchAlertTypesService;
			_commandAlertService = commandAlertService;
			_alertService = alertService;
			_randomProvider = randomProvider;
			_logger = logger;
		}

		public async Task<List<Wheel>> GetAllWheelsAsync()
		{
			var wheels = await _wheelRepository.GetAllAsync();

			return wheels
				.OrderBy(x => x.Name)
				.ToList();
		}

		public async Task<Wheel?> GetWheelAsync(string wheelId)
		{
			var wheels = await _wheelRepository.GetAllAsync();

			return wheels.FirstOrDefault(x => x.Id == wheelId);
		}

		public async Task<bool> AddWheelAsync(Wheel wheel)
		{
			if (wheel == null || string.IsNullOrWhiteSpace(wheel.Name))
			{
				return false;
			}

			wheel.Name = wheel.Name.Trim();

			var wheels = await _wheelRepository.GetAllAsync();

			// Prevent duplicate names (case-insensitive)
			if (wheels.Any(x => x.Name.Equals(wheel.Name, StringComparison.OrdinalIgnoreCase)))
			{
				return false;
			}

			wheels.Add(wheel);

			await _wheelRepository.SaveAllAsync(wheels);

			return true;
		}

		public async Task AddItemAsync(string wheelId, WheelItem item)
		{
			if (item == null || string.IsNullOrWhiteSpace(item.DisplayName))
			{
				return;
			}

			item.DisplayName = item.DisplayName.Trim();

			if (string.IsNullOrWhiteSpace(item.Id))
			{
				item.Id = Guid.NewGuid().ToString();
			}

			if (item.Weight <= 0)
			{
				item.Weight = 1;
			}

			var wheels = await _wheelRepository.GetAllAsync();

			var wheel = wheels.FirstOrDefault(x => x.Id == wheelId);

			if (wheel == null)
			{
				return;
			}

			// Assign position
			item.Position = wheel.Items.Any()
				? wheel.Items.Max(x => x.Position) + 1
				: 0;

			wheel.Items.Add(item);

			await _wheelRepository.SaveAllAsync(wheels);
		}

		public async Task RemoveItemAsync(string wheelId, string itemId)
		{
			var wheels = await _wheelRepository.GetAllAsync();

			var wheel = wheels.FirstOrDefault(x => x.Id == wheelId);

			if (wheel == null)
			{
				return;
			}

			var item = wheel.Items.FirstOrDefault(x => x.Id == itemId);

			if (item == null)
			{
				return;
			}

			wheel.Items.Remove(item);

			wheel.Items = wheel.Items
				.OrderBy(x => x.Position)
				.Select((x, index) =>
				{
					x.Position = index;
					return x;
				})
				.ToList();

			await _wheelRepository.SaveAllAsync(wheels);
		}

		public async Task ToggleHiddenAsync(string wheelId, string itemId)
		{
			var wheels = await _wheelRepository.GetAllAsync();

			var wheel = wheels.FirstOrDefault(x => x.Id == wheelId);

			if (wheel == null)
			{
				return;
			}

			var item = wheel.Items.FirstOrDefault(x => x.Id == itemId);

			if (item == null)
			{
				return;
			}

			item.IsHidden = !item.IsHidden;

			await _wheelRepository.SaveAllAsync(wheels);
		}

		public async Task ShuffleAsync(string wheelId)
		{
			var wheels = await _wheelRepository.GetAllAsync();

			var wheel = wheels.FirstOrDefault(x => x.Id == wheelId);

			if (wheel == null)
			{
				return;
			}

			var items = wheel.Items.ToList();

			for (var i = items.Count - 1; i > 0; i--)
			{
				var j = _randomProvider.Next(0, i + 1);

				(items[i], items[j]) = (items[j], items[i]);
			}

			for (var i = 0; i < items.Count; i++)
			{
				items[i].Position = i + 1;
			}

			wheel.Items = items;

			await _wheelRepository.SaveAllAsync(wheels);
		}

		public async Task<WheelItem?> SpinAsync(string wheelId)
		{
			lock (_spinLock)
			{
				if (_isSpinning)
				{
					return null;
				}

				_isSpinning = true;
			}

			try
			{
				var wheels = await _wheelRepository.GetAllAsync();

				var wheel = wheels.FirstOrDefault(x => x.Id == wheelId);

				if (wheel == null)
				{
					return null;
				}

				var activeItems = wheel.Items
					.Where(x => !x.IsHidden)
					.ToList();

				if (!activeItems.Any())
				{
					return null;
				}

				var totalWeight = activeItems.Sum(x => x.Weight > 0 ? x.Weight : 1);

				var roll = _randomProvider.Next(0, totalWeight);

				var current = 0;

				foreach (var item in activeItems)
				{
					var weight = item.Weight > 0 ? item.Weight : 1;

					current += weight;

					if (roll < current)
					{
						return item;
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Spin failed");
				return null;
			}
			finally
			{
				lock (_spinLock)
				{
					_isSpinning = false;
				}
			}
		}

		public async Task TriggerWheelAlertAsync(WheelItem item)
		{
			if (item == null)
			{
				return;
			}

			var message = item.DisplayName ?? "Wheel result";

			try
			{
				if (!string.IsNullOrWhiteSpace(item.AlertType))
				{
					switch (item.AlertType.ToLowerInvariant())
					{
						case "channelpoints":
							{
								if (!string.IsNullOrWhiteSpace(item.AlertKey))
								{
									await _twitchAlertTypesService.HandleChannelPointRedemptionAsync(
										AppSettings.Twitch.TWITCH_CHANNEL!,
										item.AlertKey);

									return;
								}

								_logger.LogWarning("Wheel item missing AlertKey for channelpoints");
								break;
							}
						case "commands":
							{
								if (!string.IsNullOrWhiteSpace(item.AlertKey))
								{
									await _commandAlertService.HandleCommandAsync(item.AlertKey, AppSettings.Twitch.TWITCH_USER_ID, 
										AppSettings.Twitch.TWITCH_CHANNEL!, AppSettings.Twitch.TWITCH_CHANNEL!, _twitchClientWrapper.SendMessage);
									return;
								}
								_logger.LogWarning("Wheel item missing AlertKey for command");
								break;
							}

						default:
							{
								_logger.LogWarning("Unknown AlertType: {AlertType}", item.AlertType);
								break;
							}
					}
				}

				// 🔥 FALLBACK (THIS WAS MISSING)
				_alertService.EnqueueAlert(message, null);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to execute wheel alert");

				// 🔥 STILL FALLBACK ON ERROR
				_alertService.EnqueueAlert(message, null);
			}
		}

		public async Task<bool> RenameWheelAsync(string wheelId, string newName)
		{
			if (string.IsNullOrWhiteSpace(newName))
			{
				return false;
			}

			newName = newName.Trim();

			var wheels = await _wheelRepository.GetAllAsync();

			// Prevent duplicate names (case-insensitive)
			if (wheels.Any(x => x.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
			{
				return false;
			}

			var wheel = wheels.FirstOrDefault(x => x.Id == wheelId);

			if (wheel == null)
			{
				return false;
			}

			wheel.Name = newName;

			await _wheelRepository.SaveAllAsync(wheels);

			return true;
		}

		public async Task<bool> DeleteWheelAsync(string wheelId)
		{
			var wheels = await _wheelRepository.GetAllAsync();

			var wheel = wheels.FirstOrDefault(x => x.Id == wheelId);

			if (wheel == null)
			{
				return false;
			}

			wheels.Remove(wheel);

			await _wheelRepository.SaveAllAsync(wheels);

			return true;
		}

		public async Task UpdateItemAsync(string wheelId, WheelItem updatedItem)
		{
			if (updatedItem == null)
			{
				return;
			}

			var name = updatedItem.DisplayName?.Trim();

			if (string.IsNullOrWhiteSpace(name))
			{
				return;
			}

			var wheels = await _wheelRepository.GetAllAsync();
			var wheel = wheels.FirstOrDefault(x => x.Id == wheelId);

			if (wheel == null)
			{
				return;
			}

			var existing = wheel.Items.FirstOrDefault(x => x.Id == updatedItem.Id);

			if (existing == null)
			{
				return;
			}

			existing.DisplayName = name;
			existing.Weight = updatedItem.Weight > 0 ? updatedItem.Weight : 1;
			existing.AlertType = updatedItem.AlertType;
			existing.AlertKey = updatedItem.AlertKey;
			existing.IsHidden = updatedItem.IsHidden;

			await _wheelRepository.SaveAllAsync(wheels);
		}
	}
}
