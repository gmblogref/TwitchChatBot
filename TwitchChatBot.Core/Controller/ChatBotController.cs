using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Controller
{
    public class ChatBotController
    {
        private readonly ITwitchClient _twitchClient;
        private readonly IWebHost _webHost;
        private readonly IWebSocketServer _webSocketServer;
        private readonly IAlertService _alertService;
        private readonly IStreamlabsService _streamlabsService;
        private readonly IEventSubService _eventSubService;
        private readonly ILogger<ChatBotController> _logger;

        public ChatBotController(
        ITwitchClient twitchClient,
        IWebHost webHost,
        IWebSocketServer webSocketServer,
        IAlertService alertService,
        IStreamlabsService streamlabsService,
        IEventSubService eventSubService,
        ILogger<ChatBotController> logger)
        {
            _twitchClient = twitchClient;
            _webHost = webHost;
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

                // Start the web host (for public/index.html and WebSocket)
                await _webHost.StartAsync(cancellationToken);
                _logger.LogInformation("🌐 Web server started.");

                // Start WebSocket server to communicate with front end
                _webSocketServer.Start();

                // Connect to Twitch
                _twitchClient.Connect();
                _logger.LogInformation("✅ Connected to Twitch.");

                // Connect to Streamlabs
                _streamlabsService.StartAsync(_alertService.EnqueueAlert);
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
                // Add Stop stuff here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to Stop something.");
            }
        }
    }
}
