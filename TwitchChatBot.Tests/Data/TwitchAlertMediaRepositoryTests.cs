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
	public class TwitchAlertMediaRepositoryTests
	{
		private readonly Mock<ILogger<TwitchAlertMediaRepository>> _loggerMock = new();

		private readonly TwitchAlertMediaRepository _sut;

		public TwitchAlertMediaRepositoryTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_sut = new TwitchAlertMediaRepository(_loggerMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["MediaMapFiles:TwitchAlertMedia"] = "twitch-alert-media.json",
				["MediaMapFiles:CommandAlertMedia"] = "command-alert-media.json"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		private void SetCache(TwitchAlertMediaMap map)
		{
			var field = typeof(TwitchAlertMediaRepository)
				.GetField("_twitchAlertMediaMap", BindingFlags.Instance | BindingFlags.NonPublic);

			field!.SetValue(_sut, map);
		}

		private TwitchAlertMediaMap? GetCache()
		{
			var field = typeof(TwitchAlertMediaRepository)
				.GetField("_twitchAlertMediaMap", BindingFlags.Instance | BindingFlags.NonPublic);

			return (TwitchAlertMediaMap?)field!.GetValue(_sut);
		}

		// =========================
		// FOLLOW
		// =========================

		[Fact]
		public async Task GetFollowMediaAsync_Should_ReturnMedia_When_Exists()
		{
			SetCache(new TwitchAlertMediaMap
			{
				Follow = new List<string> { "follow.mp4" }
			});

			var result = await _sut.GetFollowMediaAsync();

			result.Should().NotBeNull();
			result!.Should().ContainSingle();
			result[0].Should().Be("follow.mp4");
		}

		[Fact]
		public async Task GetFollowMediaAsync_Should_ReturnNull_When_NoMediaExists()
		{
			SetCache(new TwitchAlertMediaMap
			{
				Follow = null!
			});

			var result = await _sut.GetFollowMediaAsync();

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetFollowMediaAsync_Should_ReturnEmptyList_When_MapIsEmpty()
		{
			SetCache(new TwitchAlertMediaMap());

			var result = await _sut.GetFollowMediaAsync();

			result.Should().NotBeNull();
			result!.Should().BeEmpty();
		}

		// =========================
		// CHEER
		// =========================

		[Fact]
		public async Task GetCheerMapAsync_Should_ReturnDefault_When_NoTiers()
		{
			SetCache(new TwitchAlertMediaMap
			{
				Cheer = new Cheer
				{
					Default = "default.mp4",
					tiers = new List<Tier>()
				}
			});

			var result = await _sut.GetCheerMapAsync();

			result.Should().NotBeNull();
			result!.Default.Should().Be("default.mp4");
		}

		// =========================
		// CHANNEL POINTS
		// =========================

		[Fact]
		public async Task GetChannelPointsMapAsync_Should_ReturnMap_When_Exists()
		{
			SetCache(new TwitchAlertMediaMap
			{
				Channel_Points = new ChannelPoints
				{
					Default = "default.mp4",
					Tiers = new List<Tier>
					{
						new Tier
						{
							Title = "KOBE",
							Media = "kobe.mp4"
						}
					}
				}
			});

			var result = await _sut.GetChannelPointsMapAsync();

			result.Should().NotBeNull();
			result!.Tiers.Should().ContainSingle(x => x.Title == "KOBE");
		}

		// =========================
		// CACHE
		// =========================

		[Fact]
		public async Task GetFollowMediaAsync_Should_UseCache_When_CalledMultipleTimes()
		{
			SetCache(new TwitchAlertMediaMap
			{
				Follow = new List<string> { "follow.mp4" }
			});

			var cache = GetCache();

			await _sut.GetFollowMediaAsync();
			await _sut.GetFollowMediaAsync();

			GetCache().Should().BeSameAs(cache);
		}
	}
}