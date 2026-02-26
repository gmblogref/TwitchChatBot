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
        private readonly IWebSocketServer _webSocketServer;
        private readonly ILogger<WebHostWrapper> _logger;

        private readonly object _sync = new();

        private IHost? _host;
        private int _started; // 0 = false, 1 = true

        public WebHostWrapper(
            IWebSocketServer webSocketServer,
            ILogger<WebHostWrapper> logger)
        {
            _webSocketServer = webSocketServer;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                if (_started == 1)
                {
                    return;
                }

                if (_host == null)
                {
                    _host = BuildHost();
                }

                _started = 1;
            }

            try
            {
                await _host!.StartAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("✅ WebHost started on {BaseUrl}", AppSettings.WebHost.BaseUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ WebHostWrapper.StartAsync failed.");

                lock (_sync)
                {
                    _started = 0;

                    try { _host?.Dispose(); } catch { }
                    _host = null;
                }

                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            IHost? hostToStop;

            lock (_sync)
            {
                if (_started == 0)
                {
                    return;
                }

                _started = 0;
                hostToStop = _host;
                _host = null;
            }

            if (hostToStop == null)
            {
                return;
            }

            try
            {
                await hostToStop.StopAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("🛑 WebHost stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WebHostWrapper.StopAsync failed.");
            }
            finally
            {
                try
                {
                    hostToStop.Dispose();
                }
                catch
                {
                    // ignore
                }
            }
        }

        private IHost BuildHost()
        {
            return Host.CreateDefaultBuilder()
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
                                await _webSocketServer.HandleConnectionAsync(context, socket);
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
    }
}