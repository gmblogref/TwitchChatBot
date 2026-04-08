using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Models;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class TtsServiceTests
	{
		private readonly Mock<ILogger<TtsService>> _loggerMock = new();
		private readonly Mock<IAlertService> _alertServiceMock = new();

		private readonly TtsService _sut;

		public TtsServiceTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_sut = new TtsService(
				_loggerMock.Object,
				_alertServiceMock.Object);
		}

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["MediaBase:TwitchAlertsFolder"] = "alerts",
				["TTS:DefaultSpeaker"] = "Matthew",
				["TTS:PollyRegion"] = "us-east-1",
				["TTS:MaxChars"] = "350",
				["TTS:AllowedVoices:0"] = "Matthew",
				["TTS:AllowedVoices:1"] = "Joanna"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		// =========================
		// SPEAK
		// =========================

		[Fact]
		public async Task SpeakAsync_Should_NotThrow_When_InputIsValid()
		{
			var act = async () => await _sut.SpeakAsync("hello there", "Joanna");

			await act.Should().NotThrowAsync();
		}

		[Fact]
		public async Task SpeakAsync_Should_NotCallAlertService_When_TextIsEmpty()
		{
			await _sut.SpeakAsync("");

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert(It.IsAny<string>(), It.IsAny<string?>()),
				Times.Never);
		}

		[Fact]
		public async Task SpeakAsync_Should_NotThrow_When_InvalidVoiceProvided()
		{
			var act = async () => await _sut.SpeakAsync("hello there", "InvalidVoice");

			await act.Should().NotThrowAsync();
		}

		[Fact]
		public async Task SpeakAsync_Should_HandleException_When_DownstreamFails()
		{
			_alertServiceMock
				.Setup(x => x.EnqueueAlert(It.IsAny<string>(), It.IsAny<string?>()))
				.Throws(new Exception("fail"));

			var act = async () => await _sut.SpeakAsync("hello");

			await act.Should().NotThrowAsync();
		}
	}
}