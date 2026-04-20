using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
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

		// =========================
		// TESTS
		// =========================

		[Fact]
		public async Task GetAllAsync_Should_ReturnList()
		{
			var result = await _sut.GetAllAsync();

			result.Should().NotBeNull();
		}

		[Fact]
		public async Task SaveAllAsync_Should_SetCurrentState()
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

			var result = await _sut.GetAllAsync();

			result.Should().ContainSingle();
			result[0].Name.Should().Be("Updated");
		}
	}
}