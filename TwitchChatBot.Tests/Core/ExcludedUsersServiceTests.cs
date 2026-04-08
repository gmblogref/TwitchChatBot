using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Data.Contracts;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class ExcludedUsersServiceTests
	{
		private readonly Mock<IExcludedUsersRepository> _repoMock = new();
		private readonly Mock<ILogger<ExcludedUsersService>> _loggerMock = new();

		private readonly ExcludedUsersService _sut;

		public ExcludedUsersServiceTests()
		{
			_sut = new ExcludedUsersService(
				_repoMock.Object,
				_loggerMock.Object);
		}

		// =========================
		// IS USER EXCLUDED
		// =========================

		[Fact]
		public async Task IsUserExcludedAsync_Should_ReturnTrue_When_UserIsExcluded()
		{
			_repoMock
				.Setup(x => x.IsUserExcludedAsync("tyler", default))
				.ReturnsAsync(true);

			var result = await _sut.IsUserExcludedAsync("1", "Tyler");

			result.Should().BeTrue();
		}

		[Fact]
		public async Task IsUserExcludedAsync_Should_ReturnFalse_When_UserIsNotExcluded()
		{
			_repoMock
				.Setup(x => x.IsUserExcludedAsync("tyler", default))
				.ReturnsAsync(false);

			var result = await _sut.IsUserExcludedAsync("1", "Tyler");

			result.Should().BeFalse();
		}

		[Fact]
		public async Task IsUserExcludedAsync_Should_CallRepository_WithLowercaseUsername()
		{
			_repoMock
				.Setup(x => x.IsUserExcludedAsync(It.IsAny<string>(), default))
				.ReturnsAsync(true);

			await _sut.IsUserExcludedAsync("1", "Tyler");

			_repoMock.Verify(x =>
				x.IsUserExcludedAsync(
					It.Is<string>(u => u == "tyler"),
					default),
				Times.Once);
		}

		[Fact]
		public async Task IsUserExcludedAsync_Should_HandleMixedCaseUsername()
		{
			_repoMock
				.Setup(x => x.IsUserExcludedAsync(It.IsAny<string>(), default))
				.ReturnsAsync(true);

			var result = await _sut.IsUserExcludedAsync("1", "TyLeR");

			result.Should().BeTrue();

			_repoMock.Verify(x =>
				x.IsUserExcludedAsync(
					It.Is<string>(u => u == "tyler"),
					default),
				Times.Once);
		}

		[Fact]
		public async Task IsUserExcludedAsync_Should_NotCallRepository_When_UsernameIsEmpty()
		{
			var result = await _sut.IsUserExcludedAsync("1", "");

			result.Should().BeFalse();

			_repoMock.Verify(x =>
				x.IsUserExcludedAsync(It.IsAny<string>(), default),
				Times.Never);
		}
	}
}