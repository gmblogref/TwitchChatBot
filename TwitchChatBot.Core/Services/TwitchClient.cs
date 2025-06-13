using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

public class TwitchClientWrapper : ITwitchClient
{
    private readonly TwitchClient _client;
    private readonly ILogger<TwitchClientWrapper> _logger;

    public event EventHandler<TwitchMessageEventArgs> OnMessageReceived;

    public TwitchClientWrapper(string username, string accessToken, string channel, ILogger<TwitchClientWrapper> logger)
    {
        _logger = logger;

        var credentials = new ConnectionCredentials(username, accessToken);
        _client = new TwitchClient();
        _client.Initialize(credentials, channel);

        _client.OnMessageReceived += HandleMessageReceived;
        _client.OnConnected += (s, e) => _logger.LogInformation("✅ Twitch connected.");
    }

    public void Connect() => _client.Connect();

    public void SendMessage(string channel, string message) => _client.SendMessage(channel, message);

    private void HandleMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        OnMessageReceived?.Invoke(this, new TwitchMessageEventArgs
        {
            Channel = e.ChatMessage.Channel,
            Username = e.ChatMessage.Username,
            Message = e.ChatMessage.Message
        });
    }
}
