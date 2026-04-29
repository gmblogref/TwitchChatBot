using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Core.Utilities;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
	public class DonationAlertService : IDonationAlertService
	{
		private readonly ILogger<DonationAlertService> _logger;
		private readonly IDonationMediaRepository _donationMediaRepository;
		private readonly IAlertService _alertService;
		private readonly ITtsService _ttsService;
		private readonly IAlertHistoryService _alertHistoryService;

		public DonationAlertService(
			ILogger<DonationAlertService> logger,
			IDonationMediaRepository donationMediaRepository,
			IAlertService alertService,
			ITtsService ttsService,
			IAlertHistoryService alertHistoryService)
		{
			_logger = logger;
			_donationMediaRepository = donationMediaRepository;
			_alertService = alertService;
			_ttsService = ttsService;
			_alertHistoryService = alertHistoryService;
		}

		public async Task HandleDonationAsync(DonationEvent donation)
		{
			if (donation == null)
			{
				return;
			}

			_logger.LogInformation(
				"💰 Donation received from {User} Amount {Amount}",
				donation.DonorName,
				donation.Amount);

			var mediaMap = await _donationMediaRepository.GetDonationMapAsync();

			var tier = mediaMap?.Tiers?
				.Where(x => donation.Amount >= x.Min)
				.OrderByDescending(x => x.Min)
				.FirstOrDefault();

			var media = tier?.Media ?? mediaMap?.Default;

			var message =
				$"💰 {donation.DonorName} donated ${donation.Amount:0.00}!";

			if (!string.IsNullOrWhiteSpace(donation.Message))
			{
				message += $" {donation.Message}";
			}

			_alertHistoryService.Add(new AlertHistoryEntry
			{
				Type = AlertHistoryType.Donation,
				Display = $"Donation: {donation.DonorName} ${donation.Amount:0.00}",
				Username = donation.DonorName,
				Message = donation.Message
			});

			EnqueueAlertWithMedia(message, media);

			var voice =
				AppSettings.Voices.Follow ??
				AppSettings.TTS.DefaultSpeaker ??
				"Matthew";

			var ttsText = !string.IsNullOrWhiteSpace(donation.Message)
				? CoreHelperMethods.ForTts(donation.Message)
				: $"{donation.DonorName} donated {donation.Amount:0.00} dollars.";

			await _ttsService.SpeakAsync(ttsText, voice);
		}

		private void EnqueueAlertWithMedia(string message, string? mediaPath)
		{
			if (string.IsNullOrWhiteSpace(mediaPath))
			{
				_alertService.EnqueueAlert(message, null);
				return;
			}

			_alertService.EnqueueAlert(
				message,
				CoreHelperMethods.ToPublicMediaPath(mediaPath));
		}
	}
}
