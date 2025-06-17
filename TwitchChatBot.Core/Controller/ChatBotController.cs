using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Controller
{
    public class ChatBotController
    {
        private readonly ITwitchClientWrapper _twitchClient;
        private readonly IWebSocketServer _webSocketServer;
        private readonly IAlertService _alertService;
        private readonly IStreamlabsService _streamlabsService;
        private readonly IEventSubService _eventSubService;
        private readonly ILogger<ChatBotController> _logger;

        public ChatBotController(
        ITwitchClientWrapper twitchClient,
        IWebSocketServer webSocketServer,
        IAlertService alertService,
        IStreamlabsService streamlabsService,
        IEventSubService eventSubService,
        ILogger<ChatBotController> logger)
        {
            _twitchClient = twitchClient;
            _webSocketServer = webSocketServer;
            _alertService = alertService;
            _streamlabsService = streamlabsService;
            _eventSubService = eventSubService;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("🚀 Starting ChatBotController...");

                // Start WebSocket server to communicate with front end
                _webSocketServer.Start();

                // Connect to Twitch
                _twitchClient.Connect();
                _logger.LogInformation("✅ Connected to Twitch.");

                // Connect to Streamlabs
                _streamlabsService.Start(_alertService.EnqueueAlert);
                _logger.LogInformation("✅ Streamlabs WebSocket started.");

                // Connect to Twitch EventSub
                await _eventSubService.StartAsync(_alertService.EnqueueAlert, cancellationToken);
                _logger.LogInformation("✅ EventSub WebSocket started.");

                // Optional: Start any periodic timers like !ads
                _alertService.StartAdTimer(TimeSpan.FromMinutes(60));

                _logger.LogInformation("🎉 ChatBotController started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to start ChatBotController.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("🛑 Stopping ChatBotController...");

                // Stop periodic tasks
                _alertService.StopAdTimer();

                // Stop EventSub WebSocket
                await _eventSubService.StopAsync(cancellationToken);
                _logger.LogInformation("❎ EventSub WebSocket stopped.");

                // Stop Streamlabs WebSocket
                _streamlabsService.Stop();
                _logger.LogInformation("❎ Streamlabs WebSocket stopped.");

                // Disconnect from Twitch
                _twitchClient.Disconnect();
                _logger.LogInformation("❎ Disconnected from Twitch.");

                // Stop WebSocket server
                _webSocketServer.Stop();
                _logger.LogInformation("🌐 WebSocket server stopped.");

                _logger.LogInformation("✅ ChatBotController stopped cleanly.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during ChatBotController shutdown.");
            }
        }

    }
}
