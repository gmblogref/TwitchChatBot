using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Core.Services.Contracts;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class TwitchRoleServiceTests
	{
		private readonly Mock<ILogger<TwitchRoleService>> _loggerMock = new();
		private readonly Mock<IHelixLookupService> _helixLookupServiceMock = new();

		private readonly TwitchRoleService _sut;

		public TwitchRoleServiceTests()
		{
			_sut = new TwitchRoleService(
				_loggerMock.Object,
				_helixLookupServiceMock.Object);
		}

		// =========================
		// GET MODERATORS
		// =========================

		[Fact]
		public async Task GetModeratorsAsync_Should_ReturnModerators_When_DataExists()
		{
			_helixLookupServiceMock
				.Setup(x => x.GetModeratorLoginsAsync("broadcaster-id", default))
				.ReturnsAsync(new List<string> { "mod1", "mod2" });

			var result = await _sut.GetModeratorsAsync("broadcaster-id");

			result.Should().BeEquivalentTo(new List<string> { "mod1", "mod2" });
		}

		[Fact]
		public async Task GetModeratorsAsync_Should_ReturnEmpty_When_NoModeratorsExist()
		{
			_helixLookupServiceMock
				.Setup(x => x.GetModeratorLoginsAsync("broadcaster-id", default))
				.ReturnsAsync(new List<string>());

			var result = await _sut.GetModeratorsAsync("broadcaster-id");

			result.Should().BeEmpty();
		}

		[Fact]
		public async Task GetModeratorsAsync_Should_ReturnEmpty_When_ServiceThrows()
		{
			_helixLookupServiceMock
				.Setup(x => x.GetModeratorLoginsAsync("broadcaster-id", default))
				.ThrowsAsync(new Exception("error"));

			var result = await _sut.GetModeratorsAsync("broadcaster-id");

			result.Should().BeEmpty();
		}

		[Fact]
		public async Task GetModeratorsAsync_Should_CallHelixService()
		{
			_helixLookupServiceMock
				.Setup(x => x.GetModeratorLoginsAsync("broadcaster-id", default))
				.ReturnsAsync(new List<string>());

			await _sut.GetModeratorsAsync("broadcaster-id");

			_helixLookupServiceMock.Verify(x =>
				x.GetModeratorLoginsAsync("broadcaster-id", default),
				Times.Once);
		}

		// =========================
		// GET VIPS
		// =========================

		[Fact]
		public async Task GetVipsAsync_Should_ReturnVips_When_DataExists()
		{
			_helixLookupServiceMock
				.Setup(x => x.GetVipLoginsAsync("broadcaster-id", default))
				.ReturnsAsync(new List<string> { "vip1", "vip2" });

			var result = await _sut.GetVipsAsync("broadcaster-id");

			result.Should().BeEquivalentTo(new List<string> { "vip1", "vip2" });
		}

		[Fact]
		public async Task GetVipsAsync_Should_ReturnEmpty_When_NoVipsExist()
		{
			_helixLookupServiceMock
				.Setup(x => x.GetVipLoginsAsync("broadcaster-id", default))
				.ReturnsAsync(new List<string>());

			var result = await _sut.GetVipsAsync("broadcaster-id");

			result.Should().BeEmpty();
		}

		[Fact]
		public async Task GetVipsAsync_Should_ReturnEmpty_When_ServiceThrows()
		{
			_helixLookupServiceMock
				.Setup(x => x.GetVipLoginsAsync("broadcaster-id", default))
				.ThrowsAsync(new Exception("error"));

			var result = await _sut.GetVipsAsync("broadcaster-id");

			result.Should().BeEmpty();
		}

		[Fact]
		public async Task GetVipsAsync_Should_CallHelixService()
		{
			_helixLookupServiceMock
				.Setup(x => x.GetVipLoginsAsync("broadcaster-id", default))
				.ReturnsAsync(new List<string>());

			await _sut.GetVipsAsync("broadcaster-id");

			_helixLookupServiceMock.Verify(x =>
				x.GetVipLoginsAsync("broadcaster-id", default),
				Times.Once);
		}
	}
}