using Microsoft.Extensions.Logging;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Data.Utilities;
using TwitchChatBot.Models;

namespace TwitchChatBot.Data
{
	public class DonationMediaRepository : IDonationMediaRepository
	{
		private readonly ILogger<DonationMediaRepository> _logger;
		private readonly string _filePath;
		private DonationMediaMap? _donationMediaMap;

		public DonationMediaRepository(
			ILogger<DonationMediaRepository> logger)
		{
			_logger = logger;
			_filePath = DataHelperMethods.GetDonationMediaPath();
		}

		public async Task<DonationMediaMap?> GetDonationMapAsync(CancellationToken cancellationToken = default)
		{
			await GetDonationMediaAsync(cancellationToken);

			return _donationMediaMap;
		}

		private async Task GetDonationMediaAsync(CancellationToken cancellationToken = default)
		{
			if (_donationMediaMap != null)
			{
				if (_donationMediaMap.Tiers.Count > 0 ||
					!string.IsNullOrWhiteSpace(_donationMediaMap.Default))
				{
					return;
				}
			}

			_donationMediaMap =
				await DataHelperMethods.LoadOrCreateAsync<DonationMediaMap>(
					_filePath,
					_logger,
					AppSettings.MediaMapFiles.DonationMedia,
					() => new DonationMediaMap
					{
						Default = string.Empty,
						Tiers = new List<DonationTier>()
					},
					cancellationToken);
		}
	}
}