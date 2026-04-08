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
	public class FirstChatterMediaRepositoryTests
	{
		private readonly Mock<ILogger<FirstChatterMediaRepository>> _loggerMock = new();

		private readonly FirstChatterMediaRepository _sut;

		public FirstChatterMediaRepositoryTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_sut = new FirstChatterMediaRepository(_loggerMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["MediaMapFiles:FirstChattersMedia"] = "first-chatters.json"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		private void SetCache(params FirstChatterMediaItem[] items)
		{
			var loadedField = typeof(FirstChatterMediaRepository)
				.GetField("_isLoaded", BindingFlags.Instance | BindingFlags.NonPublic);

			var mapField = typeof(FirstChatterMediaRepository)
				.GetField("_firstChattersMediaMap", BindingFlags.Instance | BindingFlags.NonPublic);

			loadedField!.SetValue(_sut, true);
			mapField!.SetValue(_sut, new FirstChatterMediaMap
			{
				FirstChatterMediaItems = items.ToList()
			});
		}

		private FirstChatterMediaMap? GetCache()
		{
			var field = typeof(FirstChatterMediaRepository)
				.GetField("_firstChattersMediaMap", BindingFlags.Instance | BindingFlags.NonPublic);

			return (FirstChatterMediaMap?)field!.GetValue(_sut);
		}

		// =========================
		// GET MEDIA
		// =========================

		[Fact]
		public async Task GetFirstChatterMediaAsync_Should_ReturnMedia_When_UserExists()
		{
			SetCache(new FirstChatterMediaItem
			{
				UserId = "123",
				CurrentUserName = "geoff",
				Media = "media1.mp4"
			});

			var result = await _sut.GetFirstChatterMediaAsync("123", "geoff");

			result.Should().NotBeNull();
			result!.Media.Should().Be("media1.mp4");
		}

		[Fact]
		public async Task GetFirstChatterMediaAsync_Should_ReturnNull_When_UserDoesNotExist()
		{
			SetCache(new FirstChatterMediaItem
			{
				UserId = "123",
				CurrentUserName = "geoff",
				Media = "media1.mp4"
			});

			var result = await _sut.GetFirstChatterMediaAsync("456", "taylor");

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetFirstChatterMediaAsync_Should_BeCaseInsensitive_When_UsernameMatches()
		{
			SetCache(new FirstChatterMediaItem
			{
				UserId = string.Empty,
				CurrentUserName = "geoff",
				Media = "media1.mp4"
			});

			var result = await _sut.GetFirstChatterMediaAsync(string.Empty, "GeOfF");

			result.Should().NotBeNull();
			result!.Media.Should().Be("media1.mp4");
		}

		[Fact]
		public async Task GetFirstChatterMediaAsync_Should_MatchByUserId_When_UsernameDiffers()
		{
			SetCache(new FirstChatterMediaItem
			{
				UserId = "123",
				CurrentUserName = "oldname",
				Media = "media1.mp4"
			});

			var result = await _sut.GetFirstChatterMediaAsync("123", "newname");

			result.Should().NotBeNull();
			result!.Media.Should().Be("media1.mp4");
		}

		[Fact]
		public async Task GetFirstChatterMediaAsync_Should_UseCache_When_CalledMultipleTimes()
		{
			SetCache(new FirstChatterMediaItem
			{
				UserId = "123",
				CurrentUserName = "geoff",
				Media = "media1.mp4"
			});

			var cache = GetCache();

			await _sut.GetFirstChatterMediaAsync("123", "geoff");
			await _sut.GetFirstChatterMediaAsync("123", "geoff");

			GetCache().Should().BeSameAs(cache);
		}

		[Fact]
		public async Task GetFirstChatterMediaAsync_Should_ReturnNull_When_MapIsEmpty()
		{
			SetCache();

			var result = await _sut.GetFirstChatterMediaAsync("123", "geoff");

			result.Should().BeNull();
		}
	}
}