using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
	public class GiftSubBundleSuppressionService : IGiftSubBundleSuppressionService
	{
		private readonly ILogger<GiftSubBundleSuppressionService> _logger;
		private readonly Dictionary<string, GiftSubBundleSuppressionState> _activeSuppressions = new(StringComparer.OrdinalIgnoreCase);
		private readonly object _lock = new();
		private readonly TimeSpan _suppressionWindow = TimeSpan.FromSeconds(30);

		public GiftSubBundleSuppressionService(ILogger<GiftSubBundleSuppressionService> logger)
		{
			_logger = logger;
		}

		public void TrackBundle(string gifterKey, int expectedGiftCount)
		{
			if (string.IsNullOrWhiteSpace(gifterKey))
			{
				return;
			}

			if (expectedGiftCount <= 1)
			{
				return;
			}

			lock (_lock)
			{
				_activeSuppressions[gifterKey] = new GiftSubBundleSuppressionState
				{
					GifterKey = gifterKey,
					RemainingGiftCount = expectedGiftCount,
					ExpiresAtUtc = DateTime.UtcNow.Add(_suppressionWindow)
				};
			}

			_logger.LogInformation(
				"🎁 Tracking gift sub bundle suppression for {GifterKey}. Expected individual gift events: {ExpectedGiftCount}",
				gifterKey,
				expectedGiftCount);
		}

		public bool ShouldSuppressIndividualGift(string gifterKey)
		{
			if (string.IsNullOrWhiteSpace(gifterKey))
			{
				return false;
			}

			lock (_lock)
			{
				if (!_activeSuppressions.TryGetValue(gifterKey, out var suppression))
				{
					return false;
				}

				if (DateTime.UtcNow > suppression.ExpiresAtUtc)
				{
					_activeSuppressions.Remove(gifterKey);

					_logger.LogInformation(
						"🎁 Gift sub suppression expired for {GifterKey}.",
						gifterKey);

					return false;
				}

				suppression.RemainingGiftCount--;

				_logger.LogInformation(
					"🎁 Suppressed individual gift sub for {GifterKey}. Remaining suppressed gifts: {RemainingGiftCount}",
					gifterKey,
					suppression.RemainingGiftCount);

				if (suppression.RemainingGiftCount <= 0)
				{
					_activeSuppressions.Remove(gifterKey);

					_logger.LogInformation(
						"🎁 Gift sub suppression completed for {GifterKey}.",
						gifterKey);
				}

				return true;
			}
		}
	}
}