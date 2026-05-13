using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TwitchChatBot.Core.Services;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class ChannelPointRedemptionLockServiceTests
	{
		private readonly Mock<ILogger<ChannelPointRedemptionLockService>> _loggerMock;

		public ChannelPointRedemptionLockServiceTests()
		{
			_loggerMock = new Mock<ILogger<ChannelPointRedemptionLockService>>();
		}

		[Fact]
		public void TryUseRewardGroup_ShouldAllowReward_WhenRewardIsNotInLockGroup()
		{
			var service = CreateService();

			var result = service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"Some Other Reward");

			result.IsAllowed.Should().BeTrue();
			result.GroupName.Should().BeEmpty();
			result.Message.Should().BeEmpty();
		}

		[Fact]
		public void TryUseRewardGroup_ShouldAllowFirstRewardInLockGroup()
		{
			var service = CreateService();

			var result = service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"Justice For Puppies");

			result.IsAllowed.Should().BeTrue();
			result.GroupName.Should().Be("DogTreats");
			result.Message.Should().BeEmpty();
		}

		[Fact]
		public void TryUseRewardGroup_ShouldBlockSecondRewardInSameLockGroupForSameUser()
		{
			var service = CreateService();

			service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"Justice For Puppies");

			var result = service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"Triple Justice");

			result.IsAllowed.Should().BeFalse();
			result.GroupName.Should().Be("DogTreats");
			result.Message.Should().Contain("@TestUser");
			result.Message.Should().Contain("Justice For Puppies");
			result.Message.Should().Contain("Triple Justice");
		}

		[Fact]
		public void TryUseRewardGroup_ShouldNotBlockDifferentUsersInSameLockGroup()
		{
			var service = CreateService();

			var firstUserResult = service.TryUseRewardGroup(
				"user-123",
				"FirstUser",
				"Justice For Puppies");

			var secondUserResult = service.TryUseRewardGroup(
				"user-456",
				"SecondUser",
				"Triple Justice");

			firstUserResult.IsAllowed.Should().BeTrue();
			secondUserResult.IsAllowed.Should().BeTrue();
		}

		[Fact]
		public void TryUseRewardGroup_ShouldMatchRewardTitlesCaseInsensitive()
		{
			var service = CreateService();

			service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"justice for puppies");

			var result = service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"TRIPLE JUSTICE");

			result.IsAllowed.Should().BeFalse();
			result.GroupName.Should().Be("DogTreats");
		}

		[Fact]
		public void Clear_ShouldResetUsedRewardGroups()
		{
			var service = CreateService();

			service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"Justice For Puppies");

			var blockedResult = service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"Triple Justice");

			service.Clear();

			var allowedAfterClearResult = service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"Triple Justice");

			blockedResult.IsAllowed.Should().BeFalse();
			allowedAfterClearResult.IsAllowed.Should().BeTrue();
		}

		[Fact]
		public void TryUseRewardGroup_ShouldFallbackToUserName_WhenUserIdIsEmpty()
		{
			var service = CreateService();

			service.TryUseRewardGroup(
				string.Empty,
				"TestUser",
				"Justice For Puppies");

			var result = service.TryUseRewardGroup(
				string.Empty,
				"TestUser",
				"Triple Justice");

			result.IsAllowed.Should().BeFalse();
			result.GroupName.Should().Be("DogTreats");
		}

		private ChannelPointRedemptionLockService CreateService()
		{
			return new ChannelPointRedemptionLockService(_loggerMock.Object);
		}
	}
}