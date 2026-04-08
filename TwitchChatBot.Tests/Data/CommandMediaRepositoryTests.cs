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
	public class CommandMediaRepositoryTests
	{
		private readonly Mock<ILogger<CommandMediaRepository>> _loggerMock = new();

		private readonly CommandMediaRepository _sut;

		public CommandMediaRepositoryTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_sut = new CommandMediaRepository(_loggerMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["MediaMapFiles:CommandAlertMedia"] = "command-media.json"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		private void SetCache(params CommandMediaItem[] items)
		{
			var field = typeof(CommandMediaRepository)
				.GetField("_commandMediaMap", BindingFlags.Instance | BindingFlags.NonPublic);

			field!.SetValue(_sut, new CommandMediaMap
			{
				CommandMediaItems = items.ToList()
			});
		}

		private CommandMediaMap? GetCache()
		{
			var field = typeof(CommandMediaRepository)
				.GetField("_commandMediaMap", BindingFlags.Instance | BindingFlags.NonPublic);

			return (CommandMediaMap?)field!.GetValue(_sut);
		}

		// =========================
		// GET COMMAND
		// =========================

		[Fact]
		public async Task GetCommandMediaItemAsync_Should_ReturnItem_When_CommandExists()
		{
			SetCache(new CommandMediaItem
			{
				Command = "!hello",
				Text = "hello there",
				Media = "hello.mp4"
			});

			var result = await _sut.GetCommandMediaItemAsync("!hello");

			result.Should().NotBeNull();
			result!.Text.Should().Be("hello there");
			result.Media.Should().Be("hello.mp4");
		}

		[Fact]
		public async Task GetCommandMediaItemAsync_Should_ReturnNull_When_CommandDoesNotExist()
		{
			SetCache(new CommandMediaItem
			{
				Command = "!hello",
				Text = "hello there",
				Media = "hello.mp4"
			});

			var result = await _sut.GetCommandMediaItemAsync("!missing");

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetCommandMediaItemAsync_Should_BeCaseInsensitive_When_CommandExists()
		{
			SetCache(new CommandMediaItem
			{
				Command = "!hello",
				Text = "hello there",
				Media = "hello.mp4"
			});

			var result = await _sut.GetCommandMediaItemAsync("!HeLLo");

			result.Should().NotBeNull();
			result!.Command.Should().Be("!hello");
		}

		// =========================
		// GET ALL COMMAND NAMES
		// =========================

		[Fact]
		public async Task GetAllCommandNamesAsync_Should_ReturnCommands_When_MapHasData()
		{
			SetCache(
				new CommandMediaItem { Command = "!hello" },
				new CommandMediaItem { Command = "!bye" }
			);

			var result = await _sut.GetAllCommandNamesAsync();

			result.Should().NotBeNull();
			result.Should().Contain("!hello");
			result.Should().Contain("!bye");
		}

		[Fact]
		public async Task GetAllCommandNamesAsync_Should_ReturnEmpty_When_MapIsEmpty()
		{
			SetCache();

			var result = await _sut.GetAllCommandNamesAsync();

			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		// =========================
		// CACHE
		// =========================

		[Fact]
		public async Task GetCommandMediaItemAsync_Should_UseCache_When_CalledMultipleTimes()
		{
			SetCache(new CommandMediaItem
			{
				Command = "!hello",
				Text = "hello there",
				Media = "hello.mp4"
			});

			var cache = GetCache();

			await _sut.GetCommandMediaItemAsync("!hello");
			await _sut.GetCommandMediaItemAsync("!hello");

			GetCache().Should().BeSameAs(cache);
		}
	}
}