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
	public class ExcludedUsersRepositoryTests
	{
		private readonly Mock<ILogger<ExcludedUsersRepository>> _loggerMock = new();

		private readonly ExcludedUsersRepository _sut;

		public ExcludedUsersRepositoryTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_sut = new ExcludedUsersRepository(_loggerMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["MediaMapFiles:ExcludedUsersMedia"] = "excluded-users.json"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		private void SetCache(params string[] usernames)
		{
			var field = typeof(ExcludedUsersRepository)
				.GetField("_excludedUsers", BindingFlags.Instance | BindingFlags.NonPublic);

			field!.SetValue(_sut, usernames.ToHashSet(StringComparer.OrdinalIgnoreCase));
		}

		private HashSet<string>? GetCache()
		{
			var field = typeof(ExcludedUsersRepository)
				.GetField("_excludedUsers", BindingFlags.Instance | BindingFlags.NonPublic);

			return (HashSet<string>?)field!.GetValue(_sut);
		}

		// =========================
		// IS USER EXCLUDED
		// =========================

		[Fact]
		public async Task IsUserExcludedAsync_Should_ReturnTrue_When_UserExists()
		{
			SetCache("geoff");

			var result = await _sut.IsUserExcludedAsync("geoff");

			result.Should().BeTrue();
		}

		[Fact]
		public async Task IsUserExcludedAsync_Should_ReturnFalse_When_UserDoesNotExist()
		{
			SetCache("geoff");

			var result = await _sut.IsUserExcludedAsync("taylor");

			result.Should().BeFalse();
		}

		[Fact]
		public async Task IsUserExcludedAsync_Should_BeCaseInsensitive_When_UserExists()
		{
			SetCache("geoff");

			var result = await _sut.IsUserExcludedAsync("GeOfF");

			result.Should().BeTrue();
		}

		[Fact]
		public async Task IsUserExcludedAsync_Should_UseCache_When_CalledMultipleTimes()
		{
			SetCache("geoff");
			var cache = GetCache();

			await _sut.IsUserExcludedAsync("geoff");
			await _sut.IsUserExcludedAsync("geoff");

			GetCache().Should().BeSameAs(cache);
		}

		[Fact]
		public async Task IsUserExcludedAsync_Should_ReturnFalse_When_ListIsEmpty()
		{
			SetCache();

			var result = await _sut.IsUserExcludedAsync("geoff");

			result.Should().BeFalse();
		}
	}
}