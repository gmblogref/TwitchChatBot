using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            var config = BuildConfiguration();
            var services = ConfigureServices(config);

            ApplicationConfiguration.Initialize();

            var serviceProvider = services.BuildServiceProvider();

            var mainForm = serviceProvider.GetRequiredService<TwitchChatBot>();
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

            // App settings
            services.Configure<AppSettings>(config);

            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            // Core services
            services.AddSingleton<ITwitchClient, TwitchClientWrapper>();
            services.AddSingleton<TwitchChatBot>(); // Your WinForms entry form

            return services;
        }
    }
}
