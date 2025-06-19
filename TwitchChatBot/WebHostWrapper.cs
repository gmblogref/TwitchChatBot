using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot
{
    public class WebHostWrapper : IWebHostWrapper
    {
        private readonly IHost _host;

        public WebHostWrapper(string baseUrl, string webRoot, IWebSocketServer webSocketServer)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(baseUrl);
                    webBuilder.UseWebRoot(webRoot);
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
                    });
                })
                .Build();
        }


        public Task StartAsync(CancellationToken cancellationToken = default) => _host.StartAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken = default) => _host.StopAsync(cancellationToken);
    }
}
