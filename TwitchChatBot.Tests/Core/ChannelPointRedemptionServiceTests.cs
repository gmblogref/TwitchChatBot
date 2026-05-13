using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class ChannelPointRedemptionServiceTests
	{
		private readonly Mock<ILogger<ChannelPointRedemptionService>> _loggerMock;
		private readonly Mock<IChannelPointRedemptionLockService> _lockServiceMock;
		private readonly Mock<ITwitchAlertTypesService> _twitchAlertTypesServiceMock;
		private readonly Mock<IChatMessageService> _chatMessageServiceMock;
		private readonly Mock<IChannelPointRedemptionStatusService> _redemptionStatusServiceMock;

		public ChannelPointRedemptionServiceTests()
		{
			_loggerMock = new Mock<ILogger<ChannelPointRedemptionService>>();
			_lockServiceMock = new Mock<IChannelPointRedemptionLockService>();
			_twitchAlertTypesServiceMock = new Mock<ITwitchAlertTypesService>();
			_chatMessageServiceMock = new Mock<IChatMessageService>();
			_redemptionStatusServiceMock = new Mock<IChannelPointRedemptionStatusService>();

			var config = BuildConfiguration();
			AppSettings.Configuration = config;
		}

		[Fact]
		public async Task HandleRedemptionAsync_Should_CallAlertService_WhenRedemptionIsAllowed()
		{
			var service = CreateService();
			var redemption = CreateRedemption();

			_lockServiceMock
				.Setup(x => x.TryUseRewardGroup(
					redemption.UserId,
					redemption.UserName,
					redemption.RewardTitle))
				.Returns(new ChannelPointLockResult
				{
					IsAllowed = true,
					GroupName = "DogTreats"
				});

			await service.HandleRedemptionAsync(redemption);

			_twitchAlertTypesServiceMock.Verify(
				x => x.HandleChannelPointRedemptionAsync(
					redemption.UserName,
					redemption.RewardTitle),
				Times.Once);

			_chatMessageServiceMock.Verify(
				x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()),
				Times.Never);

			_redemptionStatusServiceMock.Verify(
				x => x.TryCancelRedemptionAsync(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()),
				Times.Never);
		}

		[Fact]
		public async Task HandleRedemptionAsync_Should_NotCallAlertService_WhenRedemptionIsBlocked()
		{
			var service = CreateService();
			var redemption = CreateRedemption();

			_lockServiceMock
				.Setup(x => x.TryUseRewardGroup(
					redemption.UserId,
					redemption.UserName,
					redemption.RewardTitle))
				.Returns(new ChannelPointLockResult
				{
					IsAllowed = false,
					GroupName = "DogTreats",
					Message = "@TestUser you already used a Justice treat reward this stream."
				});

			_redemptionStatusServiceMock
				.Setup(x => x.TryCancelRedemptionAsync(
					redemption.RedemptionId,
					redemption.RewardId,
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			await service.HandleRedemptionAsync(redemption);

			_twitchAlertTypesServiceMock.Verify(
				x => x.HandleChannelPointRedemptionAsync(
					It.IsAny<string>(),
					It.IsAny<string>()),
				Times.Never);
		}

		[Fact]
		public async Task HandleRedemptionAsync_Should_SendChatFeedback_WhenRedemptionIsBlocked()
		{
			var service = CreateService();
			var redemption = CreateRedemption();
			var message = "@TestUser you already used a Justice treat reward this stream.";

			_lockServiceMock
				.Setup(x => x.TryUseRewardGroup(
					redemption.UserId,
					redemption.UserName,
					redemption.RewardTitle))
				.Returns(new ChannelPointLockResult
				{
					IsAllowed = false,
					GroupName = "DogTreats",
					Message = message
				});

			_redemptionStatusServiceMock
				.Setup(x => x.TryCancelRedemptionAsync(
					redemption.RedemptionId,
					redemption.RewardId,
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			await service.HandleRedemptionAsync(redemption);

			_chatMessageServiceMock.Verify(
				x => x.SendMessage(
					AppSettings.Twitch.TWITCH_CHANNEL!,
					message),
				Times.Once);
		}

		[Fact]
		public async Task HandleRedemptionAsync_Should_AttemptCancelRefund_WhenBlockedAndStatusIsUnfulfilled()
		{
			var service = CreateService();
			var redemption = CreateRedemption();
			redemption.Status = "unfulfilled";

			_lockServiceMock
				.Setup(x => x.TryUseRewardGroup(
					redemption.UserId,
					redemption.UserName,
					redemption.RewardTitle))
				.Returns(new ChannelPointLockResult
				{
					IsAllowed = false,
					GroupName = "DogTreats",
					Message = "Blocked message"
				});

			_redemptionStatusServiceMock
				.Setup(x => x.TryCancelRedemptionAsync(
					redemption.RedemptionId,
					redemption.RewardId,
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			await service.HandleRedemptionAsync(redemption);

			_redemptionStatusServiceMock.Verify(
				x => x.TryCancelRedemptionAsync(
					redemption.RedemptionId,
					redemption.RewardId,
					It.IsAny<CancellationToken>()),
				Times.Once);
		}

		[Fact]
		public async Task HandleRedemptionAsync_Should_AttemptCancelRefund_WhenBlockedAndStatusIsEmpty()
		{
			var service = CreateService();
			var redemption = CreateRedemption();
			redemption.Status = string.Empty;

			_lockServiceMock
				.Setup(x => x.TryUseRewardGroup(
					redemption.UserId,
					redemption.UserName,
					redemption.RewardTitle))
				.Returns(new ChannelPointLockResult
				{
					IsAllowed = false,
					GroupName = "DogTreats",
					Message = "Blocked message"
				});

			_redemptionStatusServiceMock
				.Setup(x => x.TryCancelRedemptionAsync(
					redemption.RedemptionId,
					redemption.RewardId,
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			await service.HandleRedemptionAsync(redemption);

			_redemptionStatusServiceMock.Verify(
				x => x.TryCancelRedemptionAsync(
					redemption.RedemptionId,
					redemption.RewardId,
					It.IsAny<CancellationToken>()),
				Times.Once);
		}

		[Fact]
		public async Task HandleRedemptionAsync_Should_NotAttemptCancelRefund_WhenBlockedAndStatusIsFulfilled()
		{
			var service = CreateService();
			var redemption = CreateRedemption();
			redemption.Status = "fulfilled";

			_lockServiceMock
				.Setup(x => x.TryUseRewardGroup(
					redemption.UserId,
					redemption.UserName,
					redemption.RewardTitle))
				.Returns(new ChannelPointLockResult
				{
					IsAllowed = false,
					GroupName = "DogTreats",
					Message = "Blocked message"
				});

			await service.HandleRedemptionAsync(redemption);

			_redemptionStatusServiceMock.Verify(
				x => x.TryCancelRedemptionAsync(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()),
				Times.Never);
		}

		[Fact]
		public async Task HandleRedemptionAsync_Should_NotThrow_WhenRedemptionIsNull()
		{
			var service = CreateService();

			var act = async () => await service.HandleRedemptionAsync(null!);

			await act.Should().NotThrowAsync();

			_twitchAlertTypesServiceMock.Verify(
				x => x.HandleChannelPointRedemptionAsync(
					It.IsAny<string>(),
					It.IsAny<string>()),
				Times.Never);
		}

		[Fact]
		public async Task HandleRedemptionAsync_Should_NotThrow_WhenLockServiceThrows()
		{
			var service = CreateService();
			var redemption = CreateRedemption();

			_lockServiceMock
				.Setup(x => x.TryUseRewardGroup(
					redemption.UserId,
					redemption.UserName,
					redemption.RewardTitle))
				.Throws(new InvalidOperationException("Test exception"));

			var act = async () => await service.HandleRedemptionAsync(redemption);

			await act.Should().NotThrowAsync();

			_twitchAlertTypesServiceMock.Verify(
				x => x.HandleChannelPointRedemptionAsync(
					It.IsAny<string>(),
					It.IsAny<string>()),
				Times.Never);
		}

		private ChannelPointRedemptionService CreateService()
		{
			return new ChannelPointRedemptionService(
				_loggerMock.Object,
				_lockServiceMock.Object,
				_twitchAlertTypesServiceMock.Object,
				_chatMessageServiceMock.Object,
				_redemptionStatusServiceMock.Object);
		}

		private static ChannelPointRedemptionEvent CreateRedemption()
		{
			return new ChannelPointRedemptionEvent
			{
				RedemptionId = "redemption-123",
				RewardId = "reward-456",
				RewardTitle = "Triple Justice",
				UserId = "user-789",
				UserName = "TestUser",
				UserLogin = "testuser",
				Status = "unfulfilled",
				UserInput = string.Empty
			};
		}

		private static IConfiguration BuildConfiguration()
		{
			return new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["Twitch:Channel"] = "legendofsacks"
				})
				.Build();
		}
	}
}