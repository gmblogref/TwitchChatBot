using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using TwitchChatBot.Data;
using TwitchChatBot.Models;
using Xunit;

namespace TwitchChatBot.Tests.Data
{
	public class WatchStreakRepositoryTests
	{
		private readonly Mock<ILogger<WatchStreakRepository>> _loggerMock = new();

		private readonly WatchStreakRepository _sut;

		public WatchStreakRepositoryTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_sut = new WatchStreakRepository(_loggerMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["MediaMapFiles:UserWatchStreakMedia"] = "watch-streak.json"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		private void SetCache(WatchStreakState state)
		{
			var field = typeof(WatchStreakRepository)
				.GetField("_watchStreakState", BindingFlags.Instance | BindingFlags.NonPublic);

			field!.SetValue(_sut, state);
		}

		private WatchStreakState? GetCache()
		{
			var field = typeof(WatchStreakRepository)
				.GetField("_watchStreakState", BindingFlags.Instance | BindingFlags.NonPublic);

			return (WatchStreakState?)field!.GetValue(_sut);
		}

		// =========================
		// GET STATE
		// =========================

		[Fact]
		public async Task GetStateAsync_Should_ReturnState_When_DataExists()
		{
			SetCache(new WatchStreakState
			{
				Users = new Dictionary<string, WatchStreakUserStats>(StringComparer.OrdinalIgnoreCase)
				{
					["123"] = new WatchStreakUserStats
					{
						UserId = "123",
						UserName = "geoff",
						Consecutive = 7
					}
				}
			});

			var result = await _sut.GetStateAsync();

			result.Users.Should().ContainKey("123");
			result.Users["123"].Consecutive.Should().Be(7);
		}

		[Fact]
		public async Task GetStateAsync_Should_NotContainUser_When_UserDoesNotExist()
		{
			SetCache(new WatchStreakState
			{
				Users = new Dictionary<string, WatchStreakUserStats>(StringComparer.OrdinalIgnoreCase)
				{
					["123"] = new WatchStreakUserStats
					{
						UserId = "123",
						UserName = "geoff",
						Consecutive = 7
					}
				}
			});

			var result = await _sut.GetStateAsync();

			result.Users.ContainsKey("999").Should().BeFalse();
		}

		[Fact]
		public async Task GetStateAsync_Should_BeCaseInsensitive_When_UserExists()
		{
			SetCache(new WatchStreakState
			{
				Users = new Dictionary<string, WatchStreakUserStats>(StringComparer.OrdinalIgnoreCase)
				{
					["geoff"] = new WatchStreakUserStats
					{
						UserId = "123",
						UserName = "geoff",
						Consecutive = 7
					}
				}
			});

			var result = await _sut.GetStateAsync();

			result.Users["GeOfF"].Consecutive.Should().Be(7);
		}

		[Fact]
		public async Task GetStateAsync_Should_UseCache_When_CalledMultipleTimes()
		{
			SetCache(new WatchStreakState
			{
				Users = new Dictionary<string, WatchStreakUserStats>(StringComparer.OrdinalIgnoreCase)
				{
					["123"] = new WatchStreakUserStats
					{
						UserId = "123",
						UserName = "geoff",
						Consecutive = 7
					}
				}
			});

			var cache = GetCache();

			await _sut.GetStateAsync();
			await _sut.GetStateAsync();

			GetCache().Should().BeSameAs(cache);
		}

		[Fact]
		public async Task GetStateAsync_Should_ReturnEmptyState_When_NoUsersExist()
		{
			SetCache(new WatchStreakState
			{
				Users = new Dictionary<string, WatchStreakUserStats>(StringComparer.OrdinalIgnoreCase)
			});

			var result = await _sut.GetStateAsync();

			result.Users.Should().NotBeNull();
			result.Users.Should().BeEmpty();
		}
	}
}