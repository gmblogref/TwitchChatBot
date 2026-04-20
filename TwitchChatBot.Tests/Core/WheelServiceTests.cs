using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TwitchChatBot.Core.Providers;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class WheelServiceTests
	{
		private readonly Mock<IWheelRepository> _wheelRepositoryMock = new();
		private readonly Mock<ITwitchClientWrapper> _twitchClientWrapperMock = new();
		private readonly Mock<ITwitchAlertTypesService> _twitchAlertTypeMock = new();
		private readonly Mock<ICommandAlertService> _commandAlertServiceMock = new();
		private readonly Mock<IAlertService> _alertServiceMock = new();
		private readonly Mock<IRandomProvider> _randomProviderMock = new();
		private readonly Mock<ILogger<WheelService>> _loggerMock = new();

		private readonly WheelService _sut;

		public WheelServiceTests()
		{
			// 🔥 REQUIRED CONFIG FOR TESTS
			AppSettings.Configuration = CreateConfiguration();

			_sut = new WheelService(
				_wheelRepositoryMock.Object,
				_twitchClientWrapperMock.Object,
				_twitchAlertTypeMock.Object,
				_commandAlertServiceMock.Object,
				_alertServiceMock.Object,
				_randomProviderMock.Object,
				_loggerMock.Object);
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
			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

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
			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var item = new WheelItem { Id = "new", DisplayName = "New Item" };

			await _sut.AddItemAsync("wheel1", item);

			wheels[0].Items.Should().Contain(x => x.Id == "new");
			_wheelRepositoryMock.Verify(x => x.SaveAllAsync(wheels, default), Times.Once);
		}

		// =========================
		// REMOVE ITEM
		// =========================

		[Fact]
		public async Task RemoveItemAsync_Should_Remove_Item()
		{
			var wheels = CreateWheel(new WheelItem { Id = "1" });
			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

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

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			await _sut.ToggleHiddenAsync("wheel1", "1");

			item.IsHidden.Should().BeTrue();
		}

		// =========================
		// SPIN
		// =========================

		[Fact]
		public async Task SpinAsync_Should_Return_Item_When_Items_Exist()
		{
			var wheels = CreateWheel(new WheelItem { Id = "1", Weight = 1 });

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

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

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var result = await _sut.SpinAsync("wheel1");

			result!.Id.Should().Be("2");
		}

		[Fact]
		public async Task SpinAsync_Should_Return_Null_When_No_Active_Items()
		{
			var wheels = CreateWheel(
				new WheelItem { Id = "1", IsHidden = true }
			);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

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
				AlertType = "channelpoints",
				AlertKey = "KOBE"
			};

			await _sut.TriggerWheelAlertAsync(item);

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
				AlertType = "channelpoints",
				AlertKey = ""
			};

			await _sut.TriggerWheelAlertAsync(item);

			_twitchAlertTypeMock.Verify(x =>
				x.HandleChannelPointRedemptionAsync(
					It.IsAny<string>(),
					It.IsAny<string>()),
				Times.Never);
		}

		[Fact]
		public async Task AddWheelAsync_Should_Add_Wheel_When_Valid()
		{
			var wheels = new List<Wheel>();

			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(wheels);

			var wheel = new Wheel { Name = "TestWheel" };

			var result = await _sut.AddWheelAsync(wheel);

			result.Should().BeTrue();
			wheels.Should().Contain(wheel);

			_wheelRepositoryMock.Verify(x =>
				x.SaveAllAsync(wheels, default), Times.Once);
		}

		[Fact]
		public async Task AddWheelAsync_Should_Fail_When_Name_Is_Empty()
		{
			var result = await _sut.AddWheelAsync(new Wheel { Name = "" });

			result.Should().BeFalse();

			_wheelRepositoryMock.Verify(x =>
				x.SaveAllAsync(It.IsAny<List<Wheel>>(), default), Times.Never);
		}

		[Fact]
		public async Task AddWheelAsync_Should_Fail_On_Duplicate_Name_CaseInsensitive()
		{
			var wheels = new List<Wheel>
			{
				new Wheel { Name = "Baseball" }
			};

			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(wheels);

			var result = await _sut.AddWheelAsync(new Wheel { Name = "baseball" });

			result.Should().BeFalse();
		}

		[Fact]
		public async Task RenameWheelAsync_Should_Rename_Wheel()
		{
			var wheel = new Wheel { Id = "1", Name = "Old" };

			var wheels = new List<Wheel> { wheel };

			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(wheels);

			var result = await _sut.RenameWheelAsync("1", "New");

			result.Should().BeTrue();
			wheel.Name.Should().Be("New");

			_wheelRepositoryMock.Verify(x =>
				x.SaveAllAsync(wheels, default), Times.Once);
		}

		[Fact]
		public async Task RenameWheelAsync_Should_Fail_When_Wheel_Not_Found()
		{
			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(new List<Wheel>());

			var result = await _sut.RenameWheelAsync("1", "New");

			result.Should().BeFalse();
		}

		[Fact]
		public async Task RenameWheelAsync_Should_Fail_On_Duplicate_Name()
		{
			var wheels = new List<Wheel>
			{
				new Wheel { Id = "1", Name = "One" },
				new Wheel { Id = "2", Name = "Two" }
			};

			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(wheels);

			var result = await _sut.RenameWheelAsync("1", "two");

			result.Should().BeFalse();
		}

		[Fact]
		public async Task DeleteWheelAsync_Should_Remove_Wheel()
		{
			var wheel = new Wheel { Id = "1", Name = "Test" };

			var wheels = new List<Wheel> { wheel };

			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(wheels);

			var result = await _sut.DeleteWheelAsync("1");

			result.Should().BeTrue();
			wheels.Should().NotContain(wheel);

			_wheelRepositoryMock.Verify(x =>
				x.SaveAllAsync(wheels, default), Times.Once);
		}

		[Fact]
		public async Task DeleteWheelAsync_Should_Fail_When_Wheel_Not_Found()
		{
			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(new List<Wheel>());

			var result = await _sut.DeleteWheelAsync("1");

			result.Should().BeFalse();
		}

		// =========================
		// ADD ITEM - VALIDATION & POSITION
		// =========================

		[Fact]
		public async Task AddItemAsync_Should_Default_Weight_To_One_When_Invalid()
		{
			var wheels = CreateWheel();
			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var item = new WheelItem { Id = "1", DisplayName = "Test", Weight = 0 };

			await _sut.AddItemAsync("wheel1", item);

			item.Weight.Should().Be(1);
		}

		[Fact]
		public async Task AddItemAsync_Should_Assign_Sequential_Position()
		{
			var wheels = CreateWheel(
				new WheelItem { Id = "1", DisplayName = "A", Position = 0 }
			);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var newItem = new WheelItem { Id = "2", DisplayName = "B" };

			await _sut.AddItemAsync("wheel1", newItem);

			newItem.Position.Should().Be(1);
		}

		[Fact]
		public async Task AddItemAsync_Should_Not_Add_When_DisplayName_Invalid()
		{
			var wheels = CreateWheel();
			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var item = new WheelItem { Id = "1", DisplayName = "   " };

			await _sut.AddItemAsync("wheel1", item);

			wheels[0].Items.Should().BeEmpty();
		}

		// =========================
		// REMOVE ITEM - POSITION REBALANCE
		// =========================

		[Fact]
		public async Task RemoveItemAsync_Should_Rebalance_Positions()
		{
			var wheels = CreateWheel(
				new WheelItem { Id = "1", DisplayName = "A", Position = 0 },
				new WheelItem { Id = "2", DisplayName = "B", Position = 1 },
				new WheelItem { Id = "3", DisplayName = "C", Position = 2 }
			);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			await _sut.RemoveItemAsync("wheel1", "2");

			wheels[0].Items.Select(x => x.Position)
				.Should().BeEquivalentTo(new[] { 0, 1 });
		}

		// =========================
		// UPDATE ITEM
		// =========================

		[Fact]
		public async Task UpdateItemAsync_Should_Update_Fields()
		{
			var item = new WheelItem
			{
				Id = "1",
				DisplayName = "Old",
				Weight = 1
			};

			var wheels = CreateWheel(item);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var updated = new WheelItem
			{
				Id = "1",
				DisplayName = "New",
				Weight = 5,
				AlertType = "channelpoints",
				AlertKey = "TEST",
				IsHidden = true
			};

			await _sut.UpdateItemAsync("wheel1", updated);

			item.DisplayName.Should().Be("New");
			item.Weight.Should().Be(5);
			item.AlertType.Should().Be("channelpoints");
			item.AlertKey.Should().Be("TEST");
			item.IsHidden.Should().BeTrue();
		}

		[Fact]
		public async Task UpdateItemAsync_Should_Not_Update_When_Name_Invalid()
		{
			var item = new WheelItem
			{
				Id = "1",
				DisplayName = "Original"
			};

			var wheels = CreateWheel(item);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var updated = new WheelItem
			{
				Id = "1",
				DisplayName = "   "
			};

			await _sut.UpdateItemAsync("wheel1", updated);

			item.DisplayName.Should().Be("Original");
		}

		[Fact]
		public async Task UpdateItemAsync_Should_Default_Weight_When_Invalid()
		{
			var item = new WheelItem
			{
				Id = "1",
				DisplayName = "Test",
				Weight = 5
			};

			var wheels = CreateWheel(item);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var updated = new WheelItem
			{
				Id = "1",
				DisplayName = "Updated",
				Weight = 0
			};

			await _sut.UpdateItemAsync("wheel1", updated);

			item.Weight.Should().Be(1);
		}

		// =========================
		// SPIN - WEIGHT BEHAVIOR
		// =========================

		[Fact]
		public async Task SpinAsync_Should_Respect_Weight_Distribution()
		{
			var heavy = new WheelItem { Id = "1", DisplayName = "Heavy", Weight = 100 };
			var light = new WheelItem { Id = "2", DisplayName = "Light", Weight = 1 };

			var wheels = CreateWheel(heavy, light);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			var heavyCount = 0;

			for (int i = 0; i < 50; i++)
			{
				var result = await _sut.SpinAsync("wheel1");

				if (result?.Id == "1")
				{
					heavyCount++;
				}
			}

			heavyCount.Should().BeGreaterThan(30); // strong bias toward heavy
		}

		// =========================
		// TOGGLE HIDDEN - SAVE VERIFY
		// =========================

		[Fact]
		public async Task ToggleHiddenAsync_Should_Save_Changes()
		{
			var item = new WheelItem { Id = "1", IsHidden = false };
			var wheels = CreateWheel(item);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			await _sut.ToggleHiddenAsync("wheel1", "1");

			_wheelRepositoryMock.Verify(x => x.SaveAllAsync(wheels, default), Times.Once);
		}

		[Fact]
		public async Task SpinAsync_Should_Be_Deterministic_With_RandomProvider()
		{
			var item1 = new WheelItem { Id = "1", Weight = 1 };
			var item2 = new WheelItem { Id = "2", Weight = 1 };

			var wheels = CreateWheel(item1, item2);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			// Force roll = 0 → should pick first item
			_randomProviderMock.Setup(x => x.Next(0, 2)).Returns(0);

			var result = await _sut.SpinAsync("wheel1");

			result!.Id.Should().Be("1");
		}

		[Fact]
		public async Task SpinAsync_Should_Select_Second_Item_When_Roll_In_Second_Range()
		{
			var item1 = new WheelItem { Id = "1", Weight = 1 };
			var item2 = new WheelItem { Id = "2", Weight = 1 };

			var wheels = CreateWheel(item1, item2);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			// Force roll = 1 → should pick second item
			_randomProviderMock.Setup(x => x.Next(0, 2)).Returns(1);

			var result = await _sut.SpinAsync("wheel1");

			result!.Id.Should().Be("2");
		}

		[Fact]
		public async Task SpinAsync_Should_Default_Invalid_Weights_To_One()
		{
			var item1 = new WheelItem { Id = "1", Weight = 0 };
			var item2 = new WheelItem { Id = "2", Weight = -5 };

			var wheels = CreateWheel(item1, item2);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			_randomProviderMock.Setup(x => x.Next(0, 2)).Returns(1);

			var result = await _sut.SpinAsync("wheel1");

			result.Should().NotBeNull();
		}

		[Fact]
		public async Task SpinAsync_Should_Always_Return_Single_Item_When_Only_One()
		{
			var item = new WheelItem { Id = "only", Weight = 100 };

			var wheels = CreateWheel(item);

			_wheelRepositoryMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(wheels);

			_randomProviderMock.Setup(x => x.Next(0, 100)).Returns(50);

			var result = await _sut.SpinAsync("wheel1");

			result!.Id.Should().Be("only");
		}

		[Fact]
		public async Task SpinAsync_Should_Prevent_Concurrent_Spins()
		{
			var item = new WheelItem { Id = "1", Weight = 1 };
			var wheels = CreateWheel(item);

			var tcs = new TaskCompletionSource<List<Wheel>>();

			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.Returns(() => tcs.Task);

			_randomProviderMock
				.Setup(x => x.Next(It.IsAny<int>(), It.IsAny<int>()))
				.Returns(0);

			var task1 = _sut.SpinAsync("wheel1");

			// Let task1 enter and grab the lock
			await Task.Delay(50);

			var task2 = _sut.SpinAsync("wheel1");

			// Now release task1
			tcs.SetResult(wheels);

			var result1 = await task1;
			var result2 = await task2;

			// Only ONE should succeed
			(result1 != null ^ result2 != null).Should().BeTrue();
		}

		[Fact]
		public async Task SpinAsync_Should_Log_Error_When_Exception_Occurs()
		{
			_wheelRepositoryMock
			.Setup(x => x.GetAllAsync(default))
			.ThrowsAsync(new Exception("boom"));

			var result = await _sut.SpinAsync("wheel1");

			result.Should().BeNull();

			_loggerMock.Verify(
				x => x.Log(
					It.IsAny<LogLevel>(),
					It.IsAny<EventId>(),
					It.IsAny<It.IsAnyType>(),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
				Times.AtLeastOnce);
		}

		[Fact]
		public async Task ExecuteAsync_Should_Trigger_Mapped_Alert_When_Configured()
		{
			var item = new WheelItem
			{
				DisplayName = "Reward",
				AlertType = "channelpoints",
				AlertKey = "TEST"
			};

			await _sut.TriggerWheelAlertAsync(item);

			_twitchAlertTypeMock.Verify(x =>
				x.HandleChannelPointRedemptionAsync(
					"TestChannel",
					"TEST"),
				Times.Once);

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert(It.IsAny<string>(), It.IsAny<string>()),
				Times.Never);
		}

		[Fact]
		public async Task ExecuteAsync_Should_Fallback_To_Text_Alert_When_No_AlertType()
		{
			var item = new WheelItem
			{
				DisplayName = "Fallback Only"
			};

			await _sut.TriggerWheelAlertAsync(item);

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert("Fallback Only", null),
				Times.Once);

			_twitchAlertTypeMock.Verify(x =>
				x.HandleChannelPointRedemptionAsync(
					It.IsAny<string>(),
					It.IsAny<string>()),
				Times.Never);
		}

		[Fact]
		public async Task ExecuteAsync_Should_Fallback_When_AlertKey_Is_Missing()
		{
			var item = new WheelItem
			{
				DisplayName = "Missing Key",
				AlertType = "channelpoints",
				AlertKey = ""
			};

			await _sut.TriggerWheelAlertAsync(item);

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert("Missing Key", null),
				Times.Once);
		}

		[Fact]
		public async Task ExecuteAsync_Should_Fallback_When_AlertType_Is_Invalid()
		{
			var item = new WheelItem
			{
				DisplayName = "Invalid Type",
				AlertType = "unknown",
				AlertKey = "Test"
			};

			await _sut.TriggerWheelAlertAsync(item);

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert("Invalid Type", null),
				Times.Once);
		}

		[Fact]
		public async Task ExecuteAsync_Should_Trigger_Exactly_One_Alert()
		{
			var item = new WheelItem
			{
				DisplayName = "Single Alert"
			};

			await _sut.TriggerWheelAlertAsync(item);

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert(It.IsAny<string>(), It.IsAny<string>()),
				Times.Once);
		}

		[Fact]
		public async Task ExecuteAsync_Should_Not_Throw_And_Should_Fallback_On_Exception()
		{
			var item = new WheelItem
			{
				Id = "1",
				DisplayName = "Error Case",
				AlertType = "channelpoints",
				AlertKey = "TEST"
			};

			_twitchAlertTypeMock
				.Setup(x => x.HandleChannelPointRedemptionAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ThrowsAsync(new Exception("boom"));

			await _sut.TriggerWheelAlertAsync(item);

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert("Error Case", null),
				Times.Once);
		}

		[Fact]
		public async Task ShuffleAsync_Should_Not_Lose_Items()
		{
			var wheels = CreateWheel(
				new WheelItem { Id = "1" },
				new WheelItem { Id = "2" },
				new WheelItem { Id = "3" }
			);

			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(wheels);

			await _sut.ShuffleAsync("wheel1");

			wheels[0].Items.Should().HaveCount(3);
			wheels[0].Items.Select(x => x.Id)
				.Should()
				.BeEquivalentTo(new[] { "1", "2", "3" });
		}

		[Fact]
		public async Task ShuffleAsync_Should_Not_Duplicate_Items()
		{
			var wheels = CreateWheel(
				new WheelItem { Id = "1" },
				new WheelItem { Id = "2" },
				new WheelItem { Id = "3" }
			);

			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(wheels);

			await _sut.ShuffleAsync("wheel1");

			wheels[0].Items
				.Select(x => x.Id)
				.Distinct()
				.Count()
				.Should()
				.Be(3);
		}

		[Fact]
		public async Task ShuffleAsync_Should_Reassign_Positions_Sequentially()
		{
			var wheels = CreateWheel(
				new WheelItem { Id = "1", Position = 99 },
				new WheelItem { Id = "2", Position = 99 },
				new WheelItem { Id = "3", Position = 99 }
			);

			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(wheels);

			await _sut.ShuffleAsync("wheel1");

			wheels[0].Items
				.Select(x => x.Position)
				.Should()
				.Equal(1, 2, 3);
		}

		[Fact]
		public async Task ShuffleAsync_Should_Save_Changes()
		{
			var wheels = CreateWheel(
				new WheelItem { Id = "1" },
				new WheelItem { Id = "2" }
			);

			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(wheels);

			await _sut.ShuffleAsync("wheel1");

			_wheelRepositoryMock.Verify(
				x => x.SaveAllAsync(wheels, default),
				Times.Once);
		}

		[Fact]
		public async Task ShuffleAsync_Should_Do_Nothing_When_Wheel_Not_Found()
		{
			var wheels = CreateWheel(
				new WheelItem { Id = "1" }
			);

			_wheelRepositoryMock
				.Setup(x => x.GetAllAsync(default))
				.ReturnsAsync(wheels);

			await _sut.ShuffleAsync("missing-wheel");

			_wheelRepositoryMock.Verify(
				x => x.SaveAllAsync(It.IsAny<List<Wheel>>(), default),
				Times.Never);
		}
	}
}