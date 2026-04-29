using Microsoft.Extensions.Logging;
using SocketIOClient;
using System.Text.Json;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.UI.Services
{
	public class StreamlabsDonationSocketService : IStreamlabsDonationProviderService
	{
		private readonly ILogger<StreamlabsDonationSocketService> _logger;
		private readonly IDonationAlertService _donationAlertService;

		private SocketIO? _socket;
		private bool _isRunning;

		public StreamlabsDonationSocketService(
			ILogger<StreamlabsDonationSocketService> logger,
			IDonationAlertService donationAlertService)
		{
			_logger = logger;
			_donationAlertService = donationAlertService;
		}

		public async Task StartAsync(CancellationToken cancellationToken = default)
		{
			if (_isRunning)
			{
				return;
			}

			var token = AppSettings.Streamlabs.SocketToken;

			if (string.IsNullOrWhiteSpace(token))
			{
				_logger.LogWarning("Missing Streamlabs socket token.");
				return;
			}

			_socket = new SocketIO(new Uri($"{AppSettings.Streamlabs.SocketUrl}?token={token}"));

			_socket.OnConnected += (_, _) =>
			{
				_logger.LogInformation("Connected to Streamlabs donations.");
			};

			_socket.On("event", async response =>
			{
				await HandleEventAsync(response);
			});

			await _socket.ConnectAsync();

			_isRunning = true;
		}

		public async Task StopAsync(CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("🛑 Stopping Streamlabs WebSocket...");
			if (!_isRunning)
			{
				return;
			}

			if (_socket != null)
			{
				await _socket.DisconnectAsync();
				_socket.Dispose();
				_socket = null;
			}

			_isRunning = false;
		}

		private async Task HandleEventAsync(dynamic response)
		{
			try
			{
				var json = response.ToString();

				using var doc = JsonDocument.Parse(json);
				var root = doc.RootElement;

				if (!root.TryGetProperty("type", out JsonElement typeProp))
				{
					return;
				}

				var type = typeProp.GetString();

				if (!string.Equals(type, "donation", StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				if (!root.TryGetProperty("message", out JsonElement messages))
				{
					return;
				}

				foreach (var item in messages.EnumerateArray())
				{
					var donor =
						item.TryGetProperty("name", out var nameProp)
						? nameProp.GetString()
						: "someone";

					decimal amount = 0m;

					if (item.TryGetProperty("amount", out var amountProp))
					{
						decimal.TryParse(amountProp.GetString(), out amount);
					}

					var message =
						item.TryGetProperty("message", out var msgProp)
						? msgProp.GetString()
						: null;

					await _donationAlertService.HandleDonationAsync(
						new DonationEvent
						{
							DonorName = donor ?? "someone",
							Amount = amount,
							Message = message,
							Provider = "Streamlabs"
						});
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed processing Streamlabs event.");
			}
		}
	}
}