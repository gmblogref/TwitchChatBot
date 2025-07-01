using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TwitchChatBot.Core.Services
{
    public class ChattersService : IChattersService
    {
        private readonly ILogger<ChattersService> _logger;
        private readonly HttpClient _httpClient;

        public ChattersService(ILogger<ChattersService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task<List<string>> GetCurrentChattersAsync(string channelName)
        {
            try
            {
                var url = $"https://tmi.twitch.tv/group/user/{channelName.ToLower()}/chatters";
                var response = await _httpClient.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                var chatters = new List<string>();

                foreach (var role in doc.RootElement.GetProperty("chatters").EnumerateObject())
                {
                    foreach (var user in role.Value.EnumerateArray())
                    {
                        chatters.Add(user.GetString()!);
                    }
                }

                return chatters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to retrieve chatters.");
                return new List<string>();
            }
        }
    }
}