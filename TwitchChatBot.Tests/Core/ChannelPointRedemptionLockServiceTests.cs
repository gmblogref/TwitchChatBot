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
		public void TryUseRewardGroup_ShouldAllowReward_WhenRewardIsJusticeForPuppies()
		{
			var service = CreateService();

			var result = service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"Justice For Puppies");

			result.IsAllowed.Should().BeTrue();
			result.GroupName.Should().BeEmpty();
			result.Message.Should().BeEmpty();
		}

		[Fact]
		public void TryUseRewardGroup_ShouldAllowTripleJustice_WhenSameUserAlreadyUsedJusticeForPuppies()
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

			result.IsAllowed.Should().BeTrue();
			result.GroupName.Should().BeEmpty();
			result.Message.Should().BeEmpty();
		}

		[Fact]
		public void TryUseRewardGroup_ShouldAllowJusticeForPuppies_WhenSameUserAlreadyUsedTripleJustice()
		{
			var service = CreateService();

			service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"Triple Justice");

			var result = service.TryUseRewardGroup(
				"user-123",
				"TestUser",
				"Justice For Puppies");

			result.IsAllowed.Should().BeTrue();
			result.GroupName.Should().BeEmpty();
			result.Message.Should().BeEmpty();
		}

		[Fact]
		public void TryUseRewardGroup_ShouldAllowDogTreatRewards_WhenRewardTitlesHaveDifferentCasing()
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

			result.IsAllowed.Should().BeTrue();
			result.GroupName.Should().BeEmpty();
			result.Message.Should().BeEmpty();
		}

		[Fact]
		public void Clear_ShouldNotBlockDogTreatRewards_WhenCalledBetweenRedemptions()
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

			blockedResult.IsAllowed.Should().BeTrue();
			allowedAfterClearResult.IsAllowed.Should().BeTrue();
		}

		[Fact]
		public void TryUseRewardGroup_ShouldAllowTripleJustice_WhenUserIdIsEmptyAndSameUserAlreadyUsedJusticeForPuppies()
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

			result.IsAllowed.Should().BeTrue();
			result.GroupName.Should().BeEmpty();
			result.Message.Should().BeEmpty();
		}

		private ChannelPointRedemptionLockService CreateService()
		{
			return new ChannelPointRedemptionLockService(_loggerMock.Object);
		}
	}
}
