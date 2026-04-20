using Microsoft.Extensions.Logging;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Data.Utilities;
using TwitchChatBot.Models;

namespace TwitchChatBot.Data
{
	public class WheelRepository : IWheelRepository
	{
		private readonly ILogger<WheelRepository> _logger;
		private readonly string _filePath;

		private List<Wheel>? _wheels;

		public WheelRepository(ILogger<WheelRepository> logger)
		{
			_logger = logger;
			_filePath = DataHelperMethods.GetWheelMediaPath();
		}

		public async Task<List<Wheel>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			await EnsureLoadedAsync(cancellationToken);
			return _wheels!;
		}

		public async Task SaveAllAsync(List<Wheel> wheels, CancellationToken cancellationToken = default)
		{
			_wheels = wheels;

			await DataHelperMethods.SaveAsync(
				_filePath,
				_wheels,
				_logger,
				AppSettings.MediaMapFiles.WheelMedia,
				cancellationToken
			);
		}

		private async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
		{
			_wheels = await DataHelperMethods.LoadOrCreateAsync<List<Wheel>>(
				_filePath,
				_logger,
				AppSettings.MediaMapFiles.WheelMedia,
				() => new List<Wheel>(),
				cancellationToken
			);
		}
	}
}