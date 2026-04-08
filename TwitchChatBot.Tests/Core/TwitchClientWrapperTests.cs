using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Runtime.Serialization;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class TwitchClientWrapperTests
	{
		private readonly Mock<ILogger<TwitchClientWrapper>> _loggerMock = new();
		private readonly Mock<ICommandAlertService> _commandAlertServiceMock = new();
		private readonly Mock<IExcludedUsersService> _excludedUsersServiceMock = new();
		private readonly Mock<IFirstChatterAlertService> _firstChatterAlertServiceMock = new();
		private readonly Mock<ITwitchRoleService> _twitchRoleServiceMock = new();
		private readonly Mock<IWatchStreakService> _watchStreakServiceMock = new();
		private readonly Mock<IIRCNoticeService> _ircNoticeServiceMock = new();
		private readonly Mock<ITwitchAlertTypesService> _twitchAlertTypesServiceMock = new();
		private readonly Mock<IHelixLookupService> _helixLookupServiceMock = new();

		private readonly TwitchClientWrapper _sut;

		public TwitchClientWrapperTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_sut = new TwitchClientWrapper(
				_loggerMock.Object,
				_commandAlertServiceMock.Object,
				_excludedUsersServiceMock.Object,
				_firstChatterAlertServiceMock.Object,
				_twitchRoleServiceMock.Object,
				_watchStreakServiceMock.Object,
				_ircNoticeServiceMock.Object,
				_twitchAlertTypesServiceMock.Object,
				_helixLookupServiceMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			return new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["Twitch:BotUsername"] = "TestBot",
					["Twitch:Channel"] = "TestChannel",
					["Twitch:BotUserId"] = "bot-id",
					["Twitch:BroadcasterUserId"] = "broadcaster-id",
					["Twitch:TwitchUrl"] = "https://twitch.tv/",
					["Auth:OAuthToken"] = "oauth:test",
					["Ads:DefaultUserName"] = "TestUser"
				})
				.Build();
		}

		private async Task InvokePrivateAsync(string methodName, params object[] args)
		{
			var method = typeof(TwitchClientWrapper)
				.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

			var task = (Task)method!.Invoke(_sut, args)!;
			await task;
		}

		private void InvokePrivate(string methodName, params object[] args)
		{
			var method = typeof(TwitchClientWrapper)
				.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

			method!.Invoke(_sut, args);
		}

		private void AddRoleUser(string fieldName, string username)
		{
			var field = typeof(TwitchClientWrapper)
				.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

			var users = (HashSet<string>)field!.GetValue(_sut)!;
			users.Add(username);
		}

#pragma warning disable SYSLIB0050
		private OnMessageReceivedArgs MessageArgs(string username, string userId, string channel, string message)
		{
			var chatMessage = (ChatMessage)FormatterServices.GetUninitializedObject(typeof(ChatMessage));
			SetAutoProperty(chatMessage, "Username", username);
			SetAutoProperty(chatMessage, "UserId", userId);
			SetAutoProperty(chatMessage, "Channel", channel);
			SetAutoProperty(chatMessage, "Message", message);
			SetAutoProperty(chatMessage, "HexColor", "#FFFFFF");

			var args = (OnMessageReceivedArgs)FormatterServices.GetUninitializedObject(typeof(OnMessageReceivedArgs));
			SetAutoProperty(args, "ChatMessage", chatMessage);

			return args;
		}
#pragma warning restore SYSLIB0050

		private void SetAutoProperty(object target, string propertyName, object? value)
		{
			var type = target.GetType();

			// Try backing field first
			var field = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

			if (field != null)
			{
				field.SetValue(target, value);
				return;
			}

			// Fallback: try property setter
			var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			if (property != null && property.CanWrite)
			{
				property.SetValue(target, value);
				return;
			}

			// Final fallback: throw clear error (instead of NullReference)
			throw new InvalidOperationException($"Unable to set property '{propertyName}' on type '{type.Name}'.");
		}

		// =========================
		// MESSAGE HANDLING
		// =========================

		[Fact]
		public async Task HandleMessageReceivedAsync_Should_TriggerCommand_When_CommandMessageIsReceived()
		{
			var args = MessageArgs("Tyler", "123", "TestChannel", "!hello");

			_excludedUsersServiceMock.Setup(x => x.IsUserExcludedAsync("123", "tyler")).ReturnsAsync(false);
			_firstChatterAlertServiceMock.Setup(x => x.HandleFirstChatAsync("123", "tyler", "Tyler", false)).ReturnsAsync(false);

			await InvokePrivateAsync("HandleMessageReceivedAsync", args);

			_commandAlertServiceMock.Verify(x =>
				x.HandleCommandAsync("!hello", "123", "tyler", "TestChannel", It.IsAny<Action<string, string>>(), false),
				Times.Once);
		}

		[Fact]
		public async Task HandleMessageReceivedAsync_Should_NotTriggerCommand_When_MessageIsNotCommand()
		{
			var args = MessageArgs("Tyler", "123", "TestChannel", "hello there");

			_excludedUsersServiceMock.Setup(x => x.IsUserExcludedAsync("123", "tyler")).ReturnsAsync(false);
			_firstChatterAlertServiceMock.Setup(x => x.HandleFirstChatAsync("123", "tyler", "Tyler", false)).ReturnsAsync(false);

			await InvokePrivateAsync("HandleMessageReceivedAsync", args);

			_commandAlertServiceMock.Verify(x =>
				x.HandleCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string, string>>(), false),
				Times.Never);
		}

		[Fact]
		public async Task HandleMessageReceivedAsync_Should_IgnoreExcludedUsers()
		{
			var args = MessageArgs("Tyler", "123", "TestChannel", "!hello");

			_excludedUsersServiceMock.Setup(x => x.IsUserExcludedAsync("123", "tyler")).ReturnsAsync(true);

			await InvokePrivateAsync("HandleMessageReceivedAsync", args);

			_commandAlertServiceMock.Verify(x => x.HandleCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string, string>>(), false), Times.Never);
		}

		[Fact]
		public async Task HandleMessageReceivedAsync_Should_TriggerFirstChatterFlow()
		{
			var args = MessageArgs("Tyler", "123", "TestChannel", "hello");

			_excludedUsersServiceMock.Setup(x => x.IsUserExcludedAsync("123", "tyler")).ReturnsAsync(false);
			_firstChatterAlertServiceMock.Setup(x => x.HandleFirstChatAsync("123", "tyler", "Tyler", false)).ReturnsAsync(true);

			await InvokePrivateAsync("HandleMessageReceivedAsync", args);

			_commandAlertServiceMock.Verify(x =>
				x.TryAutoShoutOutIfStreamerAsync("tyler", "TestChannel", It.IsAny<Action<string, string>>(), default),
				Times.Once);
		}

		// =========================
		// USER TRACKING
		// =========================

		[Fact]
		public async Task HandleOnUserJoined_Should_AddModeratorAndTrackAttendance()
		{
			AddRoleUser("_modList", "moduser");
			_helixLookupServiceMock.Setup(x => x.GetUserIdByLoginAsync("moduser", default)).ReturnsAsync("123");

			await InvokePrivateAsync("HandleOnUserJoined", "moduser");

			var result = _sut.GetGroupedViewers();

			result.Should().Contain(x => x.Username == "moduser" && x.Role == "mod");

			_watchStreakServiceMock.Verify(x => x.MarkAttendanceAsync("123", "moduser"), Times.Once);
		}

		[Fact]
		public async Task HandleOnUserJoined_Should_AddVipAndTrackAttendance()
		{
			AddRoleUser("_vipList", "vipuser");
			_helixLookupServiceMock.Setup(x => x.GetUserIdByLoginAsync("vipuser", default)).ReturnsAsync("456");

			await InvokePrivateAsync("HandleOnUserJoined", "vipuser");

			var result = _sut.GetGroupedViewers();

			result.Should().Contain(x => x.Username == "vipuser" && x.Role == "vip");

			_watchStreakServiceMock.Verify(x => x.MarkAttendanceAsync("456", "vipuser"), Times.Once);
		}

		[Fact]
		public void HandleOnUserLeft_Should_RemoveUserFromTracking()
		{
			AddRoleUser("_modList", "moduser");
			_helixLookupServiceMock.Setup(x => x.GetUserIdByLoginAsync("moduser", default)).ReturnsAsync("123");

			InvokePrivateAsync("HandleOnUserJoined", "moduser").GetAwaiter().GetResult();
			InvokePrivate("HandleOnUserLeft", "moduser");

			var result = _sut.GetGroupedViewers();

			result.Should().NotContain(x => x.Username == "moduser");
		}
	}
}