using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
	public class WheelService : IWheelService
	{
		private readonly IWheelRepository _wheelRepository;
		private readonly ITwitchAlertTypesService _twitchAlertTypesService;

		private readonly Random _random = new Random();

		public WheelService(
			IWheelRepository wheelRepository,
			ITwitchAlertTypesService twitchAlertTypesService)

		{
			_wheelRepository = wheelRepository;
			_twitchAlertTypesService = twitchAlertTypesService;
		}

		public async Task<List<Wheel>> GetAllWheelsAsync()
		{
			return await _wheelRepository.GetAllAsync();
		}

		public async Task<Wheel?> GetWheelAsync(string wheelId)
		{
			var wheels = await _wheelRepository.GetAllAsync();

			return wheels.FirstOrDefault(x => x.Id == wheelId);
		}

		public async Task AddWheelAsync(Wheel wheel)
		{
			var wheels = await _wheelRepository.GetAllAsync();

			wheels.Add(wheel);

			await _wheelRepository.SaveAllAsync(wheels);
		}

		public async Task AddItemAsync(string wheelId, WheelItem item)
		{
			var wheels = await _wheelRepository.GetAllAsync();

			var wheel = wheels.FirstOrDefault(x => x.Id == wheelId);

			if (wheel == null)
			{
				return;
			}

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

			wheel.Items = wheel.Items
				.OrderBy(x => _random.Next())
				.ToList();

			await _wheelRepository.SaveAllAsync(wheels);
		}

		public async Task<WheelItem?> SpinAsync(string wheelId)
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

			var totalWeight = activeItems.Sum(x => x.Weight);

			var roll = _random.Next(1, totalWeight + 1);

			var current = 0;

			foreach (var item in activeItems)
			{
				current += item.Weight;

				if (roll <= current)
				{
					await ExecuteAsync(item); // 🔥 THIS IS THE KEY LINE
					return item;
				}
			}

			return null;
		}

		public async Task ExecuteAsync(WheelItem item)
		{
			if (item == null)
			{
				return;
			}

			switch (item.ActionType?.ToLowerInvariant())
			{
				case "channelpoints":
					{
						if (string.IsNullOrWhiteSpace(item.ActionValue))
						{
							return;
						}

						await _twitchAlertTypesService.HandleChannelPointRedemptionAsync(
							AppSettings.Twitch.TWITCH_CHANNEL!,
							item.ActionValue);

						break;
					}
				default:
					{
						// Do nothing
						break;
					}
			}
		}
	}
}
