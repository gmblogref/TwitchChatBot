using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
	public class ChannelPointRedemptionLockService : IChannelPointRedemptionLockService
	{
		private readonly ILogger<ChannelPointRedemptionLockService> _logger;
		private readonly HashSet<string> _usedRewardGroups = new(StringComparer.OrdinalIgnoreCase);

		// TODO: Move reward lock groups to JSON configuration after mutual exclusion behavior is proven.
		private readonly List<ChannelPointRewardLockGroup> _lockGroups = new()
		{
			new ChannelPointRewardLockGroup
			{
				GroupName = "DogTreats",
				RewardTitles = new List<string>
				{
					"Justice For Puppies",
					"Triple Justice"
				}
			}
		};

		public ChannelPointRedemptionLockService(ILogger<ChannelPointRedemptionLockService> logger)
		{
			_logger = logger;
		}

		public ChannelPointLockResult TryUseRewardGroup(string userId, string userName, string rewardTitle)
		{
			var lockGroup = _lockGroups.FirstOrDefault(group =>
				group.RewardTitles.Any(title =>
					string.Equals(title, rewardTitle, StringComparison.OrdinalIgnoreCase)));

			if (lockGroup == null)
			{
				return new ChannelPointLockResult
				{
					IsAllowed = true
				};
			}

			var lockUserKey = GetLockUserKey(userId, userName);
			var lockKey = $"{lockUserKey}:{lockGroup.GroupName}";

			if (_usedRewardGroups.Contains(lockKey))
			{
				var message = $"@{userName} you already used a Justice treat reward this stream. Pick one: Justice For Puppies OR Triple Justice.";

				_logger.LogInformation(
					"Blocked channel point redemption. User={UserName}, UserId={UserId}, RewardTitle={RewardTitle}, Group={GroupName}",
					userName,
					userId,
					rewardTitle,
					lockGroup.GroupName);

				return new ChannelPointLockResult
				{
					IsAllowed = false,
					GroupName = lockGroup.GroupName,
					Message = message
				};
			}

			_usedRewardGroups.Add(lockKey);

			_logger.LogInformation(
				"Marked channel point reward lock group as used. User={UserName}, UserId={UserId}, RewardTitle={RewardTitle}, Group={GroupName}",
				userName,
				userId,
				rewardTitle,
				lockGroup.GroupName);

			return new ChannelPointLockResult
			{
				IsAllowed = true,
				GroupName = lockGroup.GroupName
			};
		}

		public void Clear()
		{
			_usedRewardGroups.Clear();

			_logger.LogInformation("Cleared channel point redemption lock groups.");
		}

		private string GetLockUserKey(string userId, string userName)
		{
			if (!string.IsNullOrWhiteSpace(userId))
			{
				return userId.Trim();
			}

			return userName.Trim();
		}
	}
}