using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class FirstChatterAlertServiceTests
	{
		private readonly Mock<ILogger<FirstChatterAlertService>> _loggerMock = new();
		private readonly Mock<IFirstChatterMediaRepository> _repoMock = new();
		private readonly Mock<IExcludedUsersService> _excludedUsersServiceMock = new();
		private readonly Mock<IAlertService> _alertServiceMock = new();
		private readonly Mock<IWatchStreakService> _watchStreakServiceMock = new();
		private readonly Mock<IAlertHistoryService> _alertHistoryServiceMock = new();
		private readonly Mock<IHelixLookupService> _helixLookupServiceMock = new();

		private readonly FirstChatterAlertService _sut;

		public FirstChatterAlertServiceTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_sut = new FirstChatterAlertService(
				_loggerMock.Object,
				_repoMock.Object,
				_excludedUsersServiceMock.Object,
				_alertServiceMock.Object,
				_watchStreakServiceMock.Object,
				_alertHistoryServiceMock.Object,
				_helixLookupServiceMock.Object,
				null! // sendMessage injected per test
			);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["Twitch:Channel"] = "TestChannel"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		private static FirstChatterMediaItem Media(string userId, string username, string media)
		{
			return new FirstChatterMediaItem
			{
				UserId = userId,
				CurrentUserName = username,
				Media = media
			};
		}

		// =========================
		// HANDLE FIRST CHAT
		// =========================

		[Fact]
		public async Task HandleFirstChatAsync_Should_TriggerAlert_When_MediaExists()
		{
			string? sentMessage = null;

			Action<string, string> sendMessage = (_, message) => sentMessage = message;

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "Tyler"))
				.ReturnsAsync(false);

			_repoMock
				.Setup(x => x.GetFirstChatterMediaAsync("1", "Tyler", default))
				.ReturnsAsync(Media("1", "Tyler", "first.mp4"));

			var sut = new FirstChatterAlertService(
				_loggerMock.Object,
				_repoMock.Object,
				_excludedUsersServiceMock.Object,
				_alertServiceMock.Object,
				_watchStreakServiceMock.Object,
				_alertHistoryServiceMock.Object,
				_helixLookupServiceMock.Object,
				sendMessage);

			var result = await sut.HandleFirstChatAsync("1", "Tyler", "Tyler");

			result.Should().BeTrue();

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert(
					It.Is<string>(m => m.Contains("@Tyler")),
					It.Is<string>(m => m.Contains("first.mp4"))),
				Times.Once);
		}

		[Fact]
		public async Task HandleFirstChatAsync_Should_SendMessage_When_NoMedia()
		{
			string? sentMessage = null;

			Action<string, string> sendMessage = (_, message) => sentMessage = message;

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "Tyler"))
				.ReturnsAsync(false);

			_repoMock
				.Setup(x => x.GetFirstChatterMediaAsync("1", "Tyler", default))
				.ReturnsAsync(new FirstChatterMediaItem
				{
					UserId = "1",
					CurrentUserName = "Tyler",
					Media = null!,
					AllowedDaysOfWeek = new List<DayOfWeek>()
				});

			var sut = new FirstChatterAlertService(
				_loggerMock.Object,
				_repoMock.Object,
				_excludedUsersServiceMock.Object,
				_alertServiceMock.Object,
				_watchStreakServiceMock.Object,
				_alertHistoryServiceMock.Object,
				_helixLookupServiceMock.Object,
				sendMessage);

			var result = await sut.HandleFirstChatAsync("1", "Tyler", "Tyler");

			result.Should().BeFalse();
			sentMessage.Should().Contain("@Tyler");

			_alertServiceMock.Verify(x => x.EnqueueAlert(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public async Task HandleFirstChatAsync_Should_SendMessage_When_UserNotFound()
		{
			string? sentMessage = null;

			Action<string, string> sendMessage = (_, message) => sentMessage = message;

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "Tyler"))
				.ReturnsAsync(false);

			_repoMock
				.Setup(x => x.GetFirstChatterMediaAsync("1", "Tyler", default))
				.ReturnsAsync((FirstChatterMediaItem?)null);

			var sut = new FirstChatterAlertService(
				_loggerMock.Object,
				_repoMock.Object,
				_excludedUsersServiceMock.Object,
				_alertServiceMock.Object,
				_watchStreakServiceMock.Object,
				_alertHistoryServiceMock.Object,
				_helixLookupServiceMock.Object,
				sendMessage);

			var result = await sut.HandleFirstChatAsync("1", "Tyler", "Tyler");

			result.Should().BeFalse();
			sentMessage.Should().Contain("@Tyler");
		}

		[Fact]
		public async Task HandleFirstChatAsync_Should_UseProvidedUsername_When_CasingDiffers()
		{
			Action<string, string> sendMessage = (_, _) => { };

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "TyLeR"))
				.ReturnsAsync(false);

			_repoMock
				.Setup(x => x.GetFirstChatterMediaAsync("1", "TyLeR", default))
				.ReturnsAsync(Media("1", "tyler", "first.mp4"));

			var sut = new FirstChatterAlertService(
				_loggerMock.Object,
				_repoMock.Object,
				_excludedUsersServiceMock.Object,
				_alertServiceMock.Object,
				_watchStreakServiceMock.Object,
				_alertHistoryServiceMock.Object,
				_helixLookupServiceMock.Object,
				sendMessage);

			var result = await sut.HandleFirstChatAsync("1", "TyLeR", "Tyler");

			result.Should().BeTrue();

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert(
					It.Is<string>(m => m.Contains("@Tyler")),
					It.IsAny<string>()),
				Times.Once);
		}

		[Fact]
		public async Task HandleFirstChatAsync_Should_DoNothing_When_UserIsExcluded()
		{
			bool called = false;

			Action<string, string> sendMessage = (_, _) => called = true;

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "Tyler"))
				.ReturnsAsync(true);

			var sut = new FirstChatterAlertService(
				_loggerMock.Object,
				_repoMock.Object,
				_excludedUsersServiceMock.Object,
				_alertServiceMock.Object,
				_watchStreakServiceMock.Object,
				_alertHistoryServiceMock.Object,
				_helixLookupServiceMock.Object,
				sendMessage);

			var result = await sut.HandleFirstChatAsync("1", "Tyler", "Tyler");

			result.Should().BeFalse();
			called.Should().BeFalse();

			_repoMock.Verify(x => x.GetFirstChatterMediaAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
		}
	}
}