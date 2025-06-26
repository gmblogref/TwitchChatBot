using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Controller;
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

            // ⚙️ Core Services
            services.TryAddSingleton<IAlertService, AlertService>();
            services.TryAddSingleton<ICommandAlertService, CommandAlertService>();
            services.TryAddSingleton<IEventSubService, EventSubSocketService>();
            services.TryAddSingleton<IExcludedUsersService, ExcludedUsersService>();
            services.TryAddSingleton<IStreamlabsService, StreamlabsSocketService>();
            services.TryAddSingleton<ITwitchAlertTypesService, TwitchAlertTypesService>();
            services.TryAddSingleton<ITwitchClientWrapper, TwitchClientWrapper>();
            services.TryAddSingleton<IWebSocketServer, WebSocketServer>();

            services.TryAddSingleton<IFirstChatterAlertService>(sp =>
            new FirstChatterAlertService(
                sp.GetRequiredService<ILogger<FirstChatterAlertService>>(),
                sp.GetRequiredService<IFirstChatterMediaRepository>(),
                sp.GetRequiredService<IExcludedUsersService>(),
                sp.GetRequiredService<IAlertService>(),
                (channel, message) =>
                {
                    var twitchClient = sp.GetRequiredService<ITwitchClientWrapper>();
                    twitchClient.SendMessage(channel, message);
                }
                ));

            // 💡 WebHost
            services.TryAddSingleton<IWebHostWrapper>(sp =>
                new WebHostWrapper(
                    baseUrl: AppSettings.WebHost.BaseUrl!,
                    webRoot: AppSettings.WebHost.WebRoot!,
                    webSocketServer: sp.GetRequiredService<IWebSocketServer>()));

            // 🧠 Register the UI Form and bridge
            services.AddSingleton<TwitchChatBot>();

            // 🧠 Register ChatBotController (depends on IUiBridge)
            services.AddSingleton<ChatBotController>();

            return services;
        }
    }
}
