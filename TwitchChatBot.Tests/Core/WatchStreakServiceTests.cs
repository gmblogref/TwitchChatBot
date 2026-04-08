using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class WatchStreakServiceTests
	{
		private readonly Mock<ILogger<WatchStreakService>> _loggerMock = new();
		private readonly Mock<IExcludedUsersService> _excludedUsersServiceMock = new();
		private readonly Mock<IAppFlags> _appFlagsMock = new();
		private readonly Mock<IWatchStreakRepository> _repoMock = new();

		private readonly WatchStreakService _sut;

		public WatchStreakServiceTests()
		{
			_appFlagsMock.Setup(x => x.IsTesting).Returns(false);

			_sut = new WatchStreakService(
				_loggerMock.Object,
				_excludedUsersServiceMock.Object,
				_appFlagsMock.Object,
				_repoMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private static WatchStreakState State(int currentStreamIndex = 0, params (string Key, WatchStreakUserStats Stats)[] users)
		{
			var state = new WatchStreakState
			{
				CurrentStreamIndex = currentStreamIndex,
				Users = new Dictionary<string, WatchStreakUserStats>(StringComparer.OrdinalIgnoreCase)
			};

			foreach (var user in users)
			{
				state.Users[user.Key] = user.Stats;
			}

			return state;
		}

		// =========================
		// MARK ATTENDANCE
		// =========================

		[Fact]
		public async Task MarkAttendanceAsync_Should_IncrementStreak_When_UserReturnsConsecutively()
		{
			var state = State(4,
				("123", new WatchStreakUserStats
				{
					UserId = "123",
					UserName = "Tyler",
					Consecutive = 2,
					TotalStreams = 2,
					LastAttendedIndex = 4
				}));

			_repoMock.Setup(x => x.GetStateAsync(default)).ReturnsAsync(state);
			_excludedUsersServiceMock.Setup(x => x.IsUserExcludedAsync("123", "Tyler")).ReturnsAsync(false);

			await _sut.BeginStreamAsync();
			await _sut.MarkAttendanceAsync("123", "Tyler");

			state.Users["123"].Consecutive.Should().Be(3);
			state.Users["123"].TotalStreams.Should().Be(3);
		}

		[Fact]
		public async Task MarkAttendanceAsync_Should_ResetStreak_When_UserMissesAStream()
		{
			var state = State(5,
				("123", new WatchStreakUserStats
				{
					UserId = "123",
					UserName = "Tyler",
					Consecutive = 4,
					TotalStreams = 4,
					LastAttendedIndex = 3
				}));

			_repoMock.Setup(x => x.GetStateAsync(default)).ReturnsAsync(state);
			_excludedUsersServiceMock.Setup(x => x.IsUserExcludedAsync("123", "Tyler")).ReturnsAsync(false);

			await _sut.BeginStreamAsync();
			await _sut.MarkAttendanceAsync("123", "Tyler");

			state.Users["123"].Consecutive.Should().Be(1);
			state.Users["123"].TotalStreams.Should().Be(5);
		}

		[Fact]
		public async Task MarkAttendanceAsync_Should_NotUpdate_When_UserIsExcluded()
		{
			var state = State();

			_repoMock.Setup(x => x.GetStateAsync(default)).ReturnsAsync(state);
			_excludedUsersServiceMock.Setup(x => x.IsUserExcludedAsync("123", "Tyler")).ReturnsAsync(true);

			await _sut.BeginStreamAsync();
			await _sut.MarkAttendanceAsync("123", "Tyler");

			state.Users.Should().BeEmpty();
		}

		[Fact]
		public async Task MarkAttendanceAsync_Should_NotIncrementTwice_When_UserAlreadyMarked()
		{
			var state = State();

			_repoMock.Setup(x => x.GetStateAsync(default)).ReturnsAsync(state);
			_excludedUsersServiceMock.Setup(x => x.IsUserExcludedAsync("123", "Tyler")).ReturnsAsync(false);

			await _sut.BeginStreamAsync();
			await _sut.MarkAttendanceAsync("123", "Tyler");
			await _sut.MarkAttendanceAsync("123", "Tyler");

			state.Users["123"].Consecutive.Should().Be(1);
		}

		// =========================
		// GET STATS
		// =========================

		[Fact]
		public async Task GetStatsTupleAsync_Should_ReturnCorrectValues()
		{
			var state = State();

			_repoMock.Setup(x => x.GetStateAsync(default)).ReturnsAsync(state);
			_excludedUsersServiceMock.Setup(x => x.IsUserExcludedAsync("123", "Tyler")).ReturnsAsync(false);

			await _sut.BeginStreamAsync();
			await _sut.MarkAttendanceAsync("123", "Tyler");

			var result = await _sut.GetStatsTupleAsync("123", "Tyler");

			result.Consecutive.Should().Be(1);
			result.Total.Should().Be(1);
		}

		// =========================
		// SAVE BEHAVIOR
		// =========================

		[Fact]
		public async Task FlushSavesAsync_Should_SaveState_When_ChangesExist()
		{
			var state = State();

			_repoMock.Setup(x => x.GetStateAsync(default)).ReturnsAsync(state);
			_excludedUsersServiceMock.Setup(x => x.IsUserExcludedAsync("123", "Tyler")).ReturnsAsync(false);

			await _sut.BeginStreamAsync();
			await _sut.MarkAttendanceAsync("123", "Tyler");
			await _sut.FlushSavesAsync();

			_repoMock.Verify(x =>
				x.SaveAsync(It.IsAny<WatchStreakState>(), It.IsAny<CancellationToken>()),
				Times.AtLeastOnce);
		}
	}
}