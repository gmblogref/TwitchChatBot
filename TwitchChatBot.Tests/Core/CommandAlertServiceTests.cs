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
	public class CommandAlertServiceTests
	{
		private readonly Mock<ILogger<CommandAlertService>> _loggerMock = new();
		private readonly Mock<ICommandMediaRepository> _repoMock = new();
		private readonly Mock<IAlertService> _alertServiceMock = new();
		private readonly Mock<IExcludedUsersService> _excludedUsersServiceMock = new();
		private readonly Mock<IWatchStreakService> _watchStreakServiceMock = new();
		private readonly Mock<IHelixLookupService> _helixLookupServiceMock = new();
		private readonly Mock<ITtsService> _ttsServiceMock = new();
		private readonly Mock<IAlertHistoryService> _alertHistoryServiceMock = new();
		private readonly Mock<ITwitchRoleService> _twitchRoleServiceMock = new();
		private readonly Mock<IModerationService> _moderationServiceMock = new();
		private readonly Mock<INukeService> _nukeServiceMock = new();
		private readonly Mock<IAppFlags> _appFlagsMock = new();
		private readonly Mock<IFirstChatterAlertService> _firstChatterAlertServiceMock = new();

		private readonly CommandAlertService _sut;

		public CommandAlertServiceTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_sut = new CommandAlertService(
				_loggerMock.Object,
				_repoMock.Object,
				_alertServiceMock.Object,
				_excludedUsersServiceMock.Object,
				_watchStreakServiceMock.Object,
				_helixLookupServiceMock.Object,
				_ttsServiceMock.Object,
				_alertHistoryServiceMock.Object,
				_twitchRoleServiceMock.Object,
				_moderationServiceMock.Object,
				_nukeServiceMock.Object,
				_appFlagsMock.Object,
				_firstChatterAlertServiceMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["Moderation:ClearSpecialAlertUsers:0"] = "admin",
				["Twitch:Channel"] = "TestChannel",
				["Twitch:BotUsername"] = "TestBot",
				["Twitch:BotUserId"] = "bot-id",
				["Twitch:BroadcasterUserId"] = "broadcaster-id",
				["Twitch:TwitchUrl"] = "https://twitch.tv/"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		private static CommandMediaItem Command(string command, string? text = null, string? media = null)
		{
			return new CommandMediaItem
			{
				Command = command,
				Text = text,
				Media = media
			};
		}

		// =========================
		// HANDLE COMMAND
		// =========================

		[Fact]
		public async Task HandleCommandAsync_Should_Execute_When_CommandExists()
		{
			var sendMessageMock = new Mock<Action<string, string>>();

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "Tyler"))
				.ReturnsAsync(false);

			_repoMock
				.Setup(x => x.GetCommandMediaItemAsync("!hello", default))
				.ReturnsAsync(Command("!hello", "hello there"));

			await _sut.HandleCommandAsync("!hello", "1", "Tyler", "TestChannel", sendMessageMock.Object);

			_repoMock.Verify(x => x.GetCommandMediaItemAsync("!hello", default), Times.Once);
			sendMessageMock.Verify(x => x("TestChannel", "hello there"), Times.Once);
		}

		[Fact]
		public async Task HandleCommandAsync_Should_DoNothing_When_CommandDoesNotExist()
		{
			var sendMessageMock = new Mock<Action<string, string>>();

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "Tyler"))
				.ReturnsAsync(false);

			_repoMock
				.Setup(x => x.GetCommandMediaItemAsync("!missing", default))
				.ReturnsAsync((CommandMediaItem?)null);

			await _sut.HandleCommandAsync("!missing", "1", "Tyler", "TestChannel", sendMessageMock.Object);

			_repoMock.Verify(x => x.GetCommandMediaItemAsync("!missing", default), Times.Once);
			sendMessageMock.Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			_alertServiceMock.Verify(x => x.EnqueueAlert(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public async Task HandleCommandAsync_Should_HandleCommandsCaseInsensitively_When_CommandExists()
		{
			var sendMessageMock = new Mock<Action<string, string>>();

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "Tyler"))
				.ReturnsAsync(false);

			_repoMock
				.Setup(x => x.GetCommandMediaItemAsync("!hello", default))
				.ReturnsAsync(Command("!hello", "hello there"));

			await _sut.HandleCommandAsync("!HeLLo", "1", "Tyler", "TestChannel", sendMessageMock.Object);

			_repoMock.Verify(x => x.GetCommandMediaItemAsync("!hello", default), Times.Once);
			sendMessageMock.Verify(x => x("TestChannel", "hello there"), Times.Once);
		}

		[Fact]
		public async Task HandleCommandAsync_Should_CallSendMessage_When_CommandHasText()
		{
			var sendMessageMock = new Mock<Action<string, string>>();

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "Tyler"))
				.ReturnsAsync(false);

			_repoMock
				.Setup(x => x.GetCommandMediaItemAsync("!hello", default))
				.ReturnsAsync(Command("!hello", "hello there"));

			await _sut.HandleCommandAsync("!hello", "1", "Tyler", "TestChannel", sendMessageMock.Object);

			sendMessageMock.Verify(x => x("TestChannel", "hello there"), Times.Once);
			_alertHistoryServiceMock.Verify(x => x.Add(It.IsAny<AlertHistoryEntry>()), Times.Once);
		}

		[Fact]
		public async Task HandleCommandAsync_Should_TriggerAlertService_When_CommandHasMedia()
		{
			var sendMessageMock = new Mock<Action<string, string>>();

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "Tyler"))
				.ReturnsAsync(false);

			_repoMock
				.Setup(x => x.GetCommandMediaItemAsync("!airhorn", default))
				.ReturnsAsync(Command("!airhorn", null, "airhorn.mp4"));

			await _sut.HandleCommandAsync("!airhorn", "1", "Tyler", "TestChannel", sendMessageMock.Object);

			_alertServiceMock.Verify(x => x.EnqueueAlert("", "/media/airhorn.mp4"), Times.Once);
			sendMessageMock.Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		[Fact]
		public async Task HandleCommandAsync_Should_NotTriggerAlertService_When_CommandHasNoMedia()
		{
			var sendMessageMock = new Mock<Action<string, string>>();

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "Tyler"))
				.ReturnsAsync(false);

			_repoMock
				.Setup(x => x.GetCommandMediaItemAsync("!hello", default))
				.ReturnsAsync(Command("!hello", "hello there", null));

			await _sut.HandleCommandAsync("!hello", "1", "Tyler", "TestChannel", sendMessageMock.Object);

			_alertServiceMock.Verify(x => x.EnqueueAlert(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
			sendMessageMock.Verify(x => x("TestChannel", "hello there"), Times.Once);
		}

		[Fact]
		public async Task HandleCommandAsync_Should_DoNothing_When_CommandTextIsEmpty()
		{
			var sendMessageMock = new Mock<Action<string, string>>();

			_excludedUsersServiceMock
				.Setup(x => x.IsUserExcludedAsync("1", "Tyler"))
				.ReturnsAsync(false);

			await _sut.HandleCommandAsync("", "1", "Tyler", "TestChannel", sendMessageMock.Object);

			_repoMock.Verify(x => x.GetCommandMediaItemAsync(It.IsAny<string>(), default), Times.Never);
			sendMessageMock.Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			_alertServiceMock.Verify(x => x.EnqueueAlert(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
		}
	}
}
