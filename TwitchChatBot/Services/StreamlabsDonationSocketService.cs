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

		private CancellationTokenSource? _cts;
		private Task? _runTask;

		public StreamlabsDonationSocketService(
			ILogger<StreamlabsDonationSocketService> logger,
			IDonationAlertService donationAlertService)
		{
			_logger = logger;
			_donationAlertService = donationAlertService;
		}

		public Task StartAsync(CancellationToken cancellationToken = default)
		{
			if (_isRunning)
			{
				return Task.CompletedTask;
			}

			_isRunning = true;

			_logger.LogInformation("▶️ Starting Streamlabs donation service.");

			_cts = new CancellationTokenSource();

			_runTask = Task.Run(async () =>
			{
				try
				{
					await RunAsync(_cts.Token);
				}
				catch (OperationCanceledException)
				{
					_logger.LogInformation("Streamlabs donation service canceled.");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Streamlabs donation service crashed.");
				}
			}, _cts.Token);

			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("🛑 Stopping Streamlabs WebSocket...");

			if (!_isRunning)
			{
				return;
			}

			_isRunning = false;

			try
			{
				_cts?.Cancel();

				if (_runTask != null)
				{
					await _runTask;
				}
			}
			catch (OperationCanceledException)
			{
				// expected
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while stopping donation service.");
			}

			if (_socket != null)
			{
				try
				{
					await _socket.DisconnectAsync();
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Error during socket disconnect.");
				}

				_socket.Dispose();
				_socket = null;
			}

			_logger.LogInformation("✅ Streamlabs donation service stopped.");
		}

		private async Task RunAsync(CancellationToken cancellationToken)
		{
			var token = AppSettings.Streamlabs.SocketToken;

			if (string.IsNullOrWhiteSpace(token))
			{
				_logger.LogWarning("Missing Streamlabs socket token.");
				return;
			}

			var uri = new Uri($"{AppSettings.Streamlabs.SocketUrl}?token={token}");

			_socket = new SocketIO(uri);

			_socket.OnConnected += (_, _) =>
			{
				_logger.LogInformation("🔌 Connected to Streamlabs donations.");
			};

			_socket.OnDisconnected += (_, _) =>
			{
				_logger.LogWarning("⚠️ Disconnected from Streamlabs.");
			};

			_socket.On("event", async response =>
			{
				await HandleEventAsync(response);
			});

			await _socket.ConnectAsync();

			// Keep alive loop
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(1000, cancellationToken);
			}
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