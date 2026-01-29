using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using TwitchChatBot.Core.Controller;
using TwitchChatBot.Core.Logging;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;
using TwitchChatBot.UI.Services;

namespace TwitchChatBot
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            var config = BuildConfiguration();
            AppSettings.Configuration = config;

            var services = ConfigureServices(config);

            ApplicationConfiguration.Initialize();

            var serviceProvider = services.BuildServiceProvider();

            var mainForm = serviceProvider.GetRequiredService<TwitchChatBot>();
            var controller = serviceProvider.GetRequiredService<ChatBotController>();
            controller.SetUiBridge(mainForm); // 🔁 This breaks the circular dependency
            
            // Register the logger provider after mainForm is available
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddProvider(new TextBoxLoggerProvider(mainForm));

            Application.Run(mainForm);
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();
        }

        private static IServiceCollection ConfigureServices(IConfiguration config)
        {
            var services = new ServiceCollection();

            // 🔧 Logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            // 📦 Repositories
            services.TryAddSingleton<ICommandMediaRepository, CommandMediaRepository>();
            services.TryAddSingleton<IExcludedUsersRepository, ExcludedUsersRepository>();
            services.TryAddSingleton<IFirstChatterMediaRepository, FirstChatterMediaRepository>();
            services.TryAddSingleton<ITwitchAlertMediaRepository, TwitchAlertMediaRepository>();
            services.TryAddSingleton<IWatchStreakRepository, WatchStreakRepository>();

            // ⚙️ Core Services
            services.TryAddSingleton<IAlertService, AlertService>();
            services.TryAddSingleton<ICommandAlertService, CommandAlertService>();
            services.TryAddSingleton<IEventSubService, EventSubSocketService>();
            services.TryAddSingleton<IExcludedUsersService, ExcludedUsersService>();
            services.TryAddSingleton<ITtsService, TtsService>();
            services.TryAddSingleton<ITwitchAlertTypesService, TwitchAlertTypesService>();
            services.TryAddSingleton<ITwitchRoleService, TwitchRoleService>();
            services.TryAddSingleton<ITwitchClientWrapper, TwitchClientWrapper>();
            services.TryAddSingleton<IWebSocketServer, WebSocketServer>();
            services.TryAddSingleton<IAlertHistoryService, AlertHistoryService>(); // storage-only
            services.TryAddSingleton<IAlertReplayService, AlertReplayService>();   // dispatcher
            services.TryAddSingleton<IWatchStreakService, WatchStreakService>();
            services.TryAddSingleton<IAppFlags, AppFlags>();
            services.TryAddSingleton<INukeService, NukeService>();
            services.TryAddSingleton<IIRCNoticeService, IRCNoticeService>();
            services.TryAddSingleton<IHelixLookupService, HelixLookupService>();
            services.TryAddSingleton<IAiTextService, AiTextService>();

            services.TryAddSingleton<IFirstChatterAlertService>(sp =>
            new FirstChatterAlertService(
                sp.GetRequiredService<ILogger<FirstChatterAlertService>>(),
                sp.GetRequiredService<IFirstChatterMediaRepository>(),
                sp.GetRequiredService<IExcludedUsersService>(),
                sp.GetRequiredService<IAlertService>(),
                sp.GetRequiredService<IWatchStreakService>(),
                sp.GetRequiredService<IAlertHistoryService>(),
                sp.GetRequiredService<IHelixLookupService>(),
                (channel, message) =>
                {
                    var twitchClient = sp.GetRequiredService<ITwitchClientWrapper>();
                    twitchClient.SendMessage(channel, message);
                }
                ));

            services.AddHttpClient("twitch-helix", client =>
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", AppSettings.TWITCH_ACCESS_TOKEN);
                client.DefaultRequestHeaders.Add("Client-Id", AppSettings.TWITCH_CLIENT_ID);
            });

            services.AddHttpClient("twitch-bot-helix", client =>
            {
                // Bot bearer WITHOUT "oauth:" prefix
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", AppSettings.TWITCH_OAUTH_BEARER_BOT);
                client.DefaultRequestHeaders.Add("Client-Id", AppSettings.TWITCH_CLIENT_ID);
            });

            services.TryAddScoped<IModerationService, ModerationService>();

            services.TryAddSingleton<IWebHostWrapper, WebHostWrapper>();

            // 🧠 Register the UI Form and bridge
            services.TryAddSingleton<TwitchChatBot>();

            // 🧠 Register ChatBotController (depends on IUiBridge)
            services.TryAddSingleton<ChatBotController>();

            // 🧠 Register Tests
            services.TryAddSingleton<ITestUtilityService, TestUtilityService>();

            
            return services;
        }
    }
}
