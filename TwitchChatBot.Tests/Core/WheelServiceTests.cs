using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class WheelServiceTests
	{
		private readonly Mock<IWheelRepository> _repoMock = new();
		private readonly Mock<ITwitchAlertTypesService> _twitchAlertTypeMock = new();

		private readonly WheelService _sut;

		public WheelServiceTests()
		{
			// 🔥 REQUIRED CONFIG FOR TESTS
			AppSettings.Configuration = CreateConfiguration();

			_sut = new WheelService(
				_repoMock.Object,
				_twitchAlertTypeMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["Twitch:Channel"] = "TestChannel"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		private List<Wheel> CreateWheel(params WheelItem[] items)
		{
			return new List<Wheel>
			{
				new Wheel
				{
					Id = "wheel1",
					Name = "Test",
					Items = items.ToList()
				}
			};
		}

		// =========================
		// GET
		// =========================

		[Fact]
		public async Task GetWheelAsync_Should_Return_Wheel_When_Exists()
		{
			var wheels = CreateWheel();
			_repoMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var result = await _sut.GetWheelAsync("wheel1");

			result.Should().NotBeNull();
		}

		// =========================
		// ADD ITEM
		// =========================

		[Fact]
		public async Task AddItemAsync_Should_Add_Item_And_Save()
		{
			var wheels = CreateWheel();
			_repoMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var item = new WheelItem { Id = "new" };

			await _sut.AddItemAsync("wheel1", item);

			wheels[0].Items.Should().Contain(x => x.Id == "new");
			_repoMock.Verify(x => x.SaveAllAsync(wheels, default), Times.Once);
		}

		// =========================
		// REMOVE ITEM
		// =========================

		[Fact]
		public async Task RemoveItemAsync_Should_Remove_Item()
		{
			var wheels = CreateWheel(new WheelItem { Id = "1" });
			_repoMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			await _sut.RemoveItemAsync("wheel1", "1");

			wheels[0].Items.Should().BeEmpty();
		}

		// =========================
		// TOGGLE HIDDEN
		// =========================

		[Fact]
		public async Task ToggleHiddenAsync_Should_Flip_State()
		{
			var item = new WheelItem { Id = "1", IsHidden = false };
			var wheels = CreateWheel(item);

			_repoMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			await _sut.ToggleHiddenAsync("wheel1", "1");

			item.IsHidden.Should().BeTrue();
		}

		// =========================
		// SHUFFLE
		// =========================

		[Fact]
		public async Task ShuffleAsync_Should_Not_Lose_Items()
		{
			var wheels = CreateWheel(
				new WheelItem { Id = "1" },
				new WheelItem { Id = "2" }
			);

			_repoMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			await _sut.ShuffleAsync("wheel1");

			wheels[0].Items.Count.Should().Be(2);
		}

		// =========================
		// SPIN
		// =========================

		[Fact]
		public async Task SpinAsync_Should_Return_Item_When_Items_Exist()
		{
			var wheels = CreateWheel(new WheelItem { Id = "1", Weight = 1 });

			_repoMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var result = await _sut.SpinAsync("wheel1");

			result.Should().NotBeNull();
		}

		[Fact]
		public async Task SpinAsync_Should_Ignore_Hidden_Items()
		{
			var wheels = CreateWheel(
				new WheelItem { Id = "1", IsHidden = true },
				new WheelItem { Id = "2", IsHidden = false }
			);

			_repoMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var result = await _sut.SpinAsync("wheel1");

			result!.Id.Should().Be("2");
		}

		[Fact]
		public async Task SpinAsync_Should_Return_Null_When_No_Active_Items()
		{
			var wheels = CreateWheel(
				new WheelItem { Id = "1", IsHidden = true }
			);

			_repoMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var result = await _sut.SpinAsync("wheel1");

			result.Should().BeNull();
		}

		// =========================
		// EXECUTE
		// =========================

		[Fact]
		public async Task ExecuteAsync_ChannelPoints_Should_Call_TwitchAlertService()
		{
			var item = new WheelItem
			{
				ActionType = "channelpoints",
				ActionValue = "KOBE"
			};

			await _sut.ExecuteAsync(item);

			_twitchAlertTypeMock.Verify(x =>
				x.HandleChannelPointRedemptionAsync(
					"TestChannel",
					"KOBE"),
				Times.Once);
		}

		[Fact]
		public async Task ExecuteAsync_Should_Not_Call_When_ActionValue_Is_Empty()
		{
			var item = new WheelItem
			{
				ActionType = "channelpoints",
				ActionValue = ""
			};

			await _sut.ExecuteAsync(item);

			_twitchAlertTypeMock.Verify(x =>
				x.HandleChannelPointRedemptionAsync(
					It.IsAny<string>(),
					It.IsAny<string>()),
				Times.Never);
		}
	}
}