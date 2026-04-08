using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class TwitchAlertTypesServiceTests
	{
		private readonly Mock<ILogger<TwitchAlertTypesService>> _loggerMock = new();
		private readonly Mock<ITwitchAlertMediaRepository> _repoMock = new();
		private readonly Mock<IAlertService> _alertServiceMock = new();
		private readonly Mock<ITtsService> _ttsServiceMock = new();
		private readonly Mock<IAlertHistoryService> _alertHistoryServiceMock = new();
		private readonly Mock<IAiTextService> _aiTextServiceMock = new();

		private readonly TwitchAlertTypesService _sut;

		public TwitchAlertTypesServiceTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			_sut = new TwitchAlertTypesService(
				_loggerMock.Object,
				_repoMock.Object,
				_alertServiceMock.Object,
				_ttsServiceMock.Object,
				_alertHistoryServiceMock.Object,
				_aiTextServiceMock.Object);
		}

		// =========================
		// HELPERS
		// =========================

		private IConfiguration CreateConfiguration()
		{
			var settings = new Dictionary<string, string?>
			{
				["MediaBase:TwitchAlertsFolder"] = "alerts",

				// Voices
				["TTS:Voices:Cheer"] = "CheerVoice",
				["TTS:Voices:Follow"] = "FollowVoice",
				["TTS:Voices:ChannelPoints"] = "ChannelVoice",
				["TTS:Voices:Raid"] = "RaidVoice",
				["TTS:Voices:Subscribe"] = "SubscribeVoice",
				["TTS:Voices:SubscriptionMessage"] = "ResubVoice",
				["TTS:Voices:GiftSubs"] = "GiftVoice",
				["TTS:Voices:SingleGiftSub"] = "SingleGiftVoice",
				["TTS:Voices:WatchStreak"] = "WatchVoice",

				// Templates
				["TTS:Templates:Follow"] = "{username} followed",
				["TTS:Templates:Raid"] = "{raider} raid {viewers}",
				["TTS:Templates:CheerNoMessage"] = "{username} cheered {bits} bits!",
				["TTS:Templates:SubNoMessage"] = "{username} subscribed at tier {tier}",
				["TTS:Templates:ReSub"] = "{username} resubbed for {months} at tier {tier}",
				["TTS:Templates:MysteryGift"] = "{username} dropped {numOfSubs} tier {tier} gift subs",
				["TTS:Templates:SingleGiftSub"] = "{username} gifted {recipient} a tier {tier} sub",
				["TTS:Templates:WatchStreak"] = "{username} streak {streak}",
				
				["Ads:DefaultUserName"] = "TestUser",

				// OpenAI (IMPORTANT FOR TESTS)
				["OpenAI:DefaultAlertTone"] = "hype",
				["OpenAI:DefaultAlertMaxWords"] = "20",
				["OpenAI:WatchStreakThreshold"] = "0",
				["OpenAI:GiftSubThreshold"] = "0"
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(settings)
				.Build();
		}

		private static List<string> Media(params string[] items) => items.ToList();

		// =========================
		// CHEER
		// =========================

		[Fact]
		public async Task HandleCheerAsync_Should_UseExactTierAndSpeak_When_MessagePresent()
		{
			var cheer = new Cheer
			{
				Default = "default.mp4",
				tiers = new List<Tier>
				{
					new Tier { Min = 100, Media = "tier100.mp4" },
					new Tier { Min = 350, Media = "tier350.mp4" }
				}
			};

			_repoMock.Setup(x => x.GetCheerMapAsync(default)).ReturnsAsync(cheer);

			await _sut.HandleCheerAsync("Tyler", 350, "@chat lets go");

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert(
					It.IsAny<string>(),
					It.Is<string>(m => m.Contains("tier350.mp4"))),
				Times.Once);

			_ttsServiceMock.Verify(x =>
				x.SpeakAsync("chat lets go", "CheerVoice", null),
				Times.Once);
		}

		[Fact]
		public async Task HandleCheerAsync_Should_UseTemplateSpeech_When_MessageEmpty()
		{
			var cheer = new Cheer
			{
				Default = "default.mp4",
				tiers = new List<Tier>
				{
					new Tier { Min = 100, Media = "tier100.mp4" }
				}
			};

			_repoMock.Setup(x => x.GetCheerMapAsync(default)).ReturnsAsync(cheer);

			await _sut.HandleCheerAsync("Tyler", 250, "");

			_ttsServiceMock.Verify(x =>
				x.SpeakAsync(It.Is<string>(s => s.Contains("Tyler")), "CheerVoice", null),
				Times.Once);
		}

		// =========================
		// FOLLOW
		// =========================

		[Fact]
		public async Task HandleFollowAsync_Should_EnqueueAndSpeak_When_MediaExists()
		{
			_repoMock.Setup(x => x.GetFollowMediaAsync(default))
				.ReturnsAsync(Media("follow.mp4"));

			await _sut.HandleFollowAsync("Tyler");

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert(
					It.IsAny<string>(),
					It.Is<string>(m => m.Contains("follow.mp4"))),
				Times.Once);

			_ttsServiceMock.Verify(x =>
				x.SpeakAsync(It.IsAny<string>(), "FollowVoice", null),
				Times.Once);
		}

		// =========================
		// RAID
		// =========================

		[Fact]
		public async Task HandleRaidAsync_Should_UseAiMessage_When_Available()
		{
			_repoMock.Setup(x => x.GetRaidMediaAsync(default))
				.ReturnsAsync(Media("raid.mp4"));

			_aiTextServiceMock
				.Setup(x => x.GenerateAlertLineAsync(It.IsAny<AlertContext>()))
				.ReturnsAsync("AI line");

			await _sut.HandleRaidAsync("Tyler", 10);

			_ttsServiceMock.Verify(x =>
				x.SpeakAsync("AI line", It.IsAny<string>(), null),
				Times.Once);
		}

		// =========================
		// CHANNEL POINTS
		// =========================

		[Fact]
		public async Task HandleChannelPointRedemptionAsync_Should_PlayMedia_When_MatchExists()
		{
			var map = new ChannelPoints
			{
				Tiers = new List<Tier>
				{
					new Tier { Title = "KOBE", Media = "kobe.mp4" }
				}
			};

			_repoMock.Setup(x => x.GetChannelPointsMapAsync(default)).ReturnsAsync(map);
			_repoMock.Setup(x => x.GetChannelPointsTextMapAsync(default))
				.ReturnsAsync(new ChannelPointsText());

			await _sut.HandleChannelPointRedemptionAsync("Tyler", "KOBE");

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert(
					It.IsAny<string>(),
					It.Is<string>(m => m.Contains("kobe.mp4"))),
				Times.Once);

			_ttsServiceMock.Verify(x =>
				x.SpeakAsync(It.IsAny<string>(), It.IsAny<string?>(), null),
				Times.Never);
		}

		[Fact]
		public async Task HandleChannelPointRedemptionAsync_Should_Speak_When_TextRewardHasTts()
		{
			var textMap = new ChannelPointsText
			{
				Tiers = new List<Tier>
				{
					new Tier
					{
						Title = "Hydrate",
						Message = "[userName] says drink water",
						TtsMessage = "[userName] hydrate now"
					}
				}
			};

			_repoMock.Setup(x => x.GetChannelPointsMapAsync(default))
				.ReturnsAsync(new ChannelPoints());

			_repoMock.Setup(x => x.GetChannelPointsTextMapAsync(default))
				.ReturnsAsync(textMap);

			await _sut.HandleChannelPointRedemptionAsync("Tyler", "Hydrate");

			_alertServiceMock.Verify(x =>
				x.EnqueueAlert("Tyler says drink water", null),
				Times.Once);

			_ttsServiceMock.Verify(x =>
				x.SpeakAsync("Tyler hydrate now", "ChannelVoice", null),
				Times.Once);
		}
	}
}