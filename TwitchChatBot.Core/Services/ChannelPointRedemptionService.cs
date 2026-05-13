using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
	public class ChannelPointRedemptionService : IChannelPointRedemptionService
	{
		private readonly ILogger<ChannelPointRedemptionService> _logger;
		private readonly IChannelPointRedemptionLockService _channelPointRedemptionLockService;
		private readonly ITwitchAlertTypesService _twitchAlertTypesService;
		private readonly IChatMessageService _chatMessageService;
		private readonly IChannelPointRedemptionStatusService _channelPointRedemptionStatusService;

		public ChannelPointRedemptionService(
			ILogger<ChannelPointRedemptionService> logger,
			IChannelPointRedemptionLockService channelPointRedemptionLockService,
			ITwitchAlertTypesService twitchAlertTypesService,
			IChatMessageService chatMessageService,
			IChannelPointRedemptionStatusService channelPointRedemptionStatusService)
		{
			_logger = logger;
			_channelPointRedemptionLockService = channelPointRedemptionLockService;
			_twitchAlertTypesService = twitchAlertTypesService;
			_chatMessageService = chatMessageService;
			_channelPointRedemptionStatusService = channelPointRedemptionStatusService;
		}

		public async Task HandleRedemptionAsync(ChannelPointRedemptionEvent redemption)
		{
			try
			{
				if (redemption == null)
				{
					_logger.LogWarning("Channel point redemption was null.");
					return;
				}

				var lockResult = _channelPointRedemptionLockService.TryUseRewardGroup(
					redemption.UserId,
					redemption.UserName,
					redemption.RewardTitle);

				if (!lockResult.IsAllowed)
				{
					await HandleIsNotAllowed(redemption, lockResult);

					return;
				}

				_logger.LogInformation(
					"Channel point redemption allowed. User={UserName}, RewardTitle={RewardTitle}, Group={GroupName}",
					redemption.UserName,
					redemption.RewardTitle,
					lockResult.GroupName);

				await _twitchAlertTypesService.HandleChannelPointRedemptionAsync(
					redemption.UserName,
					redemption.RewardTitle);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Unexpected error while handling channel point redemption. User={UserName}, RewardTitle={RewardTitle}, RedemptionId={RedemptionId}, RewardId={RewardId}",
					redemption?.UserName,
					redemption?.RewardTitle,
					redemption?.RedemptionId,
					redemption?.RewardId);
			}
		}

		private async Task HandleIsNotAllowed(ChannelPointRedemptionEvent redemption, ChannelPointLockResult lockResult)
		{
			_logger.LogInformation(
					"Channel point redemption blocked. User={UserName}, RewardTitle={RewardTitle}, Group={GroupName}, Message={Message}",
					redemption.UserName,
					redemption.RewardTitle,
					lockResult.GroupName,
					lockResult.Message);

			_chatMessageService.SendMessage(
				AppSettings.Twitch.TWITCH_CHANNEL!,
				lockResult.Message);

			if (string.IsNullOrWhiteSpace(redemption.Status) ||
				string.Equals(redemption.Status, "unfulfilled", StringComparison.OrdinalIgnoreCase))
			{
				var cancelSucceeded = await _channelPointRedemptionStatusService.TryCancelRedemptionAsync(
					redemption.RedemptionId,
					redemption.RewardId);

				if (!cancelSucceeded)
				{
					_logger.LogWarning(
						"Channel point redemption was blocked locally, but Twitch cancel/refund did not succeed. User={UserName}, RewardTitle={RewardTitle}, Status={Status}, RedemptionId={RedemptionId}, RewardId={RewardId}",
						redemption.UserName,
						redemption.RewardTitle,
						redemption.Status,
						redemption.RedemptionId,
						redemption.RewardId);
				}
			}
			else
			{
				_logger.LogInformation(
					"Channel point redemption was blocked locally, but cancel/refund was skipped because status was not unfulfilled. User={UserName}, RewardTitle={RewardTitle}, Status={Status}, RedemptionId={RedemptionId}, RewardId={RewardId}",
					redemption.UserName,
					redemption.RewardTitle,
					redemption.Status,
					redemption.RedemptionId,
					redemption.RewardId);
			}
		}
	}
}