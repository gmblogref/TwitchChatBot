using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
	public class ChannelPointRedemptionStatusService : IChannelPointRedemptionStatusService
	{
		private readonly ILogger<ChannelPointRedemptionStatusService> _logger;
		private readonly IHttpClientFactory _httpClientFactory;

		public ChannelPointRedemptionStatusService(
			ILogger<ChannelPointRedemptionStatusService> logger,
			IHttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_httpClientFactory = httpClientFactory;
		}

		public async Task<bool> TryCancelRedemptionAsync(
			string redemptionId,
			string rewardId,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(redemptionId))
			{
				_logger.LogWarning("Cannot cancel channel point redemption because redemption id was empty.");
				return false;
			}

			if (string.IsNullOrWhiteSpace(rewardId))
			{
				_logger.LogWarning("Cannot cancel channel point redemption because reward id was empty.");
				return false;
			}

			if (string.IsNullOrWhiteSpace(AppSettings.Auth.TWITCH_ACCESS_TOKEN) ||
				string.IsNullOrWhiteSpace(AppSettings.Auth.TWITCH_CLIENT_ID) ||
				string.IsNullOrWhiteSpace(AppSettings.Twitch.TWITCH_USER_ID))
			{
				_logger.LogWarning("Cannot cancel channel point redemption because Twitch credentials are missing.");
				return false;
			}

			try
			{
				var httpClient = _httpClientFactory.CreateClient("twitch-helix");

				var url =
					$"https://api.twitch.tv/helix/channel_points/custom_rewards/redemptions" +
					$"?broadcaster_id={Uri.EscapeDataString(AppSettings.Twitch.TWITCH_USER_ID!)}" +
					$"&reward_id={Uri.EscapeDataString(rewardId)}" +
					$"&id={Uri.EscapeDataString(redemptionId)}";

				var body = new
				{
					status = "CANCELED"
				};

				var response = await httpClient.PatchAsJsonAsync(url, body, cancellationToken);

				if (response.IsSuccessStatusCode)
				{
					_logger.LogInformation(
						"Canceled/refunded channel point redemption. RedemptionId={RedemptionId}, RewardId={RewardId}",
						redemptionId,
						rewardId);

					return true;
				}

				var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

				_logger.LogWarning(
					"Failed to cancel/refund channel point redemption. StatusCode={StatusCode}, Body={Body}, RedemptionId={RedemptionId}, RewardId={RewardId}",
					response.StatusCode,
					responseBody,
					redemptionId,
					rewardId);

				return false;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(
					ex,
					"Exception while trying to cancel/refund channel point redemption. RedemptionId={RedemptionId}, RewardId={RewardId}",
					redemptionId,
					rewardId);

				return false;
			}
		}
	}
}