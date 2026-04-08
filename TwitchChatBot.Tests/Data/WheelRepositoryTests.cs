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
	public class WheelRepositoryTests
	{
		private readonly Mock<ILogger<WheelRepository>> _loggerMock = new();

		private readonly WheelRepository _sut;

		public WheelRepositoryTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_sut = new WheelRepository(_loggerMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["MediaMapFiles:WheelMedia"] = "wheel-media.json"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		private void SetCache(List<Wheel> wheels)
		{
			var field = typeof(WheelRepository)
				.GetField("_wheels", BindingFlags.Instance | BindingFlags.NonPublic);

			field!.SetValue(_sut, wheels);
		}

		private List<Wheel>? GetCache()
		{
			var field = typeof(WheelRepository)
				.GetField("_wheels", BindingFlags.Instance | BindingFlags.NonPublic);

			return (List<Wheel>?)field!.GetValue(_sut);
		}

		// =========================
		// GET ALL
		// =========================

		[Fact]
		public async Task GetAllAsync_Should_ReturnWheels_When_DataExists()
		{
			SetCache(new List<Wheel>
			{
				new Wheel
				{
					Id = "1",
					Name = "Main"
				}
			});

			var result = await _sut.GetAllAsync();

			result.Should().NotBeNull();
			result.Should().ContainSingle();
			result[0].Name.Should().Be("Main");
		}

		[Fact]
		public async Task GetAllAsync_Should_ReturnEmptyList_When_NoWheelsExist()
		{
			SetCache(new List<Wheel>());

			var result = await _sut.GetAllAsync();

			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		// =========================
		// SAVE
		// =========================

		[Fact]
		public async Task SaveAllAsync_Should_UpdateCache_When_Called()
		{
			var wheels = new List<Wheel>
			{
				new Wheel
				{
					Id = "1",
					Name = "Updated"
				}
			};

			await _sut.SaveAllAsync(wheels);

			var cache = GetCache();

			cache.Should().NotBeNull();
			cache![0].Name.Should().Be("Updated");
		}

		// =========================
		// CACHE
		// =========================

		[Fact]
		public async Task GetAllAsync_Should_UseCache_When_CalledMultipleTimes()
		{
			var wheels = new List<Wheel>
			{
				new Wheel { Id = "1", Name = "Main" }
			};

			SetCache(wheels);

			var cache = GetCache();

			await _sut.GetAllAsync();
			await _sut.GetAllAsync();

			GetCache().Should().BeSameAs(cache);
		}
	}
}