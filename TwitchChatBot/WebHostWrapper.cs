using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot
{
    public class WebHostWrapper : IWebHostWrapper
    {
        private readonly IHost _host;

        public WebHostWrapper(IWebSocketServer webSocketServer)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(AppSettings.WebHost.BaseUrl);
                    webBuilder.UseWebRoot(AppSettings.WebHost.WebRoot);
                    webBuilder.Configure(app =>
                    {
                        app.UseWebSockets();

                        app.Use(async (context, next) =>
                        {
                            if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
                            {
                                var socket = await context.WebSockets.AcceptWebSocketAsync();
                                await webSocketServer.HandleConnectionAsync(context, socket);
                            }
                            else
                            {
                                await next();
                            }
                        });

                        app.UseStaticFiles();

                        app.UseStaticFiles(new StaticFileOptions
                        {
                            FileProvider = new PhysicalFileProvider(AppSettings.MediaBase.TwitchAlertsFolder),
                            RequestPath = "/media"
                        });

                        // Enable endpoint routing
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/alerts", async context =>
                            {
                                var file = Path.Combine(AppSettings.WebHost.WebRoot, "alerts.html");
                                await context.Response.SendFileAsync(file);
                            });

                            endpoints.MapGet("/fullscreen", async context =>
                            {
                                var file = Path.Combine(AppSettings.WebHost.WebRoot, "fullscreen.html");
                                await context.Response.SendFileAsync(file);
                            });
                        });
                    });
                })
                .Build();
        }

        public Task StartAsync(CancellationToken cancellationToken = default) => _host.StartAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken = default) => _host.StopAsync(cancellationToken);
    }
}
