using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class AlertServiceTests
	{
		private readonly Mock<ILogger<AlertService>> _loggerMock = new();
		private readonly Mock<IWebSocketServer> _webSocketServerMock = new();

		private readonly AlertService _sut;

		public AlertServiceTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_webSocketServerMock.Setup(x => x.HasClientsConnected).Returns(true);

			_sut = new AlertService(
				_loggerMock.Object,
				_webSocketServerMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["AlertSettings:AlertTimeOut"] = "1",
				["AlertSettings:AcknowledgeTimeOut"] = "1",
				["AlertSettings:MaxQueueSize"] = "10",
				["AlertSettings:MaxOfflineQueueTime"] = "30",
				["AlertSettings:RetryTaskDelay"] = "10",
				["MediaBase:TwitchAlertsFolder"] = "alerts"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		private static string GetPayloadProperty(object payload, string propertyName)
		{
			var property = payload.GetType().GetProperty(propertyName);
			return (string)property!.GetValue(payload)!;
		}

		private async Task WaitForAsync(Func<bool> condition)
		{
			for (var i = 0; i < 100; i++)
			{
				if (condition())
				{
					return;
				}

				await Task.Delay(20);
			}

			throw new Exception("Condition not met in time");
		}

		// =========================
		// ENQUEUE ALERT
		// =========================

		[Fact]
		public async Task EnqueueAlert_Should_SendAlert_When_Added()
		{
			string? alertId = null;

			_webSocketServerMock
				.Setup(x => x.BroadcastAsync(It.IsAny<object>()))
				.Callback<object>(payload =>
				{
					alertId = GetPayloadProperty(payload, "alertId");

					_webSocketServerMock.Raise(x => x.OnClientAck += null, alertId);
					_webSocketServerMock.Raise(x => x.OnClientDone += null, alertId);
				})
				.Returns(Task.CompletedTask);

			_sut.EnqueueAlert("hello");

			await WaitForAsync(() => alertId != null);

			_webSocketServerMock.Verify(x => x.BroadcastAsync(It.IsAny<object>()), Times.Once);
		}

		[Fact]
		public async Task EnqueueAlert_Should_ProcessInOrder_When_MultipleAlertsQueued()
		{
			var messages = new List<string>();

			_webSocketServerMock
				.Setup(x => x.BroadcastAsync(It.IsAny<object>()))
				.Callback<object>(payload =>
				{
					var alertId = GetPayloadProperty(payload, "alertId");

					messages.Add(GetPayloadProperty(payload, "message"));

					_webSocketServerMock.Raise(x => x.OnClientAck += null, alertId);
					_webSocketServerMock.Raise(x => x.OnClientDone += null, alertId);
				})
				.Returns(Task.CompletedTask);

			_sut.EnqueueAlert("first");
			_sut.EnqueueAlert("second");

			await WaitForAsync(() => messages.Count == 2);

			messages.Should().Equal("first", "second");
		}

		[Fact]
		public async Task EnqueueAlert_Should_NotSend_When_NoClientsConnected()
		{
			_webSocketServerMock.Setup(x => x.HasClientsConnected).Returns(false);

			_sut.EnqueueAlert("hello");

			await Task.Delay(50);

			_webSocketServerMock.Verify(x => x.BroadcastAsync(It.IsAny<object>()), Times.Never);
		}

		[Fact]
		public async Task EnqueueAlert_Should_CompleteProcessing_When_DoneSignalReceived()
		{
			string? alertId = null;

			_webSocketServerMock
				.Setup(x => x.BroadcastAsync(It.IsAny<object>()))
				.Callback<object>(payload =>
				{
					alertId = GetPayloadProperty(payload, "alertId");

					_webSocketServerMock.Raise(x => x.OnClientAck += null, alertId);
					_webSocketServerMock.Raise(x => x.OnClientDone += null, alertId);
				})
				.Returns(Task.CompletedTask);

			_sut.EnqueueAlert("hello");

			await WaitForAsync(() => alertId != null);

			_webSocketServerMock.Verify(x => x.BroadcastAsync(It.IsAny<object>()), Times.Once);
		}

		[Fact]
		public async Task EnqueueAlert_Should_Retry_When_BroadcastFails()
		{
			var attempts = 0;

			_webSocketServerMock
				.Setup(x => x.BroadcastAsync(It.IsAny<object>()))
				.Callback<object>(payload =>
				{
					attempts++;

					if (attempts > 1)
					{
						var alertId = GetPayloadProperty(payload, "alertId");

						_webSocketServerMock.Raise(x => x.OnClientAck += null, alertId);
						_webSocketServerMock.Raise(x => x.OnClientDone += null, alertId);
					}
				})
				.Returns<object>(_ =>
				{
					if (attempts == 0)
					{
						throw new Exception("fail");
					}

					return Task.CompletedTask;
				});

			_sut.EnqueueAlert("retry");

			await WaitForAsync(() => attempts >= 2);

			attempts.Should().BeGreaterThan(1);
		}
	}
}