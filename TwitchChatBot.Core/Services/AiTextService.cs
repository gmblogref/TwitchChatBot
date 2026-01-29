using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class AiTextService : IAiTextService
    {
        private readonly HttpClient _http;

        public AiTextService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(AppSettings.OpenAI.BaseUrl)
            };

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AppSettings.OpenAI.ApiKey);
        }

        public async Task<string?> GenerateAlertLineAsync(AlertContext context)
        {
            switch (context.AiType)
            {
                case AlertAiType.Raid:
                    return await GenerateRaidContext(context);
                case AlertAiType.WatchStreak:
                    return await GenerateWatchStreakContext(context);
                case AlertAiType.ResubMilestone:
                    return await GenerateResubMilestoneContext(context);
                case AlertAiType.GiftSubBomb:
                    return await GenerateGiftSubBombContext(context);
                default:
                    return null;
            }
        }

        private async Task<string?> GenerateRaidContext(AlertContext context)
        {
            var prompt = @$"Generate a Twitch raid hype message.
                
                        Raider: {context.Username}
                        Viewers: {context.ViewerCount}
                        
                        Rules:
                        - Max {context.MaxWords} words
                        - No emojis
                        - Make it energetic and thankful
                        - Spoken aloud friendly";

            return await GenerateFromPromptAsync(prompt, context.MaxWords);
        }

        private async Task<string?> GenerateWatchStreakContext(AlertContext context)
        {
            var prompt = @$"Generate a Twitch watch streak hype message.
                        
                        Viewer: {context.Username}
                        Streak: {context.StreakCount} streams
                        
                        Rules:
                        - Max {context.MaxWords} words
                        - Celebratory
                        - No emojis
                        - Spoken aloud friendly";

            return await GenerateFromPromptAsync(prompt, context.MaxWords);
        }

        private async Task<string?> GenerateResubMilestoneContext(AlertContext context)
        {
            var prompt = @$"Generate a Twitch resub milestone hype message.
                        
                        User: {context.Username}
                        Years: {(context.Months ?? 0) / 12}
                        Tier: {context.Tier}

                        Rules:
                        - Max {context.MaxWords} words
                        - No emojis
                        - Spoken aloud friendly
                        - Mention the milestone year count if possible (months/12)
                        ";

            return await GenerateFromPromptAsync(prompt, context.MaxWords);
        }

        private async Task<string?> GenerateGiftSubBombContext(AlertContext context)
        {
            var prompt = @$"Generate a Twitch hype message for a big gift sub drop.
                
                        Gifter: {context.Username}
                        GiftSubs: {context.GiftCount}
                        Tier: {context.Tier}
                
                        Rules:
                        - Max {context.MaxWords} words
                        - No emojis
                        - Spoken aloud friendly
                        - Make it energetic and thankful";

            return await GenerateFromPromptAsync(prompt, context.MaxWords);
        }

        private async Task<string?> GenerateFromPromptAsync(string prompt, int maxWords)
        {
            var body = new
            {
                model = AppSettings.OpenAI.Model,
                input = prompt
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _http.PostAsync("responses", content);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                foreach (var output in doc.RootElement.GetProperty("output").EnumerateArray())
                {
                    foreach (var block in output.GetProperty("content").EnumerateArray())
                    {
                        if (block.GetProperty("type").GetString() == "output_text")
                        {
                            return Sanitize(block.GetProperty("text").GetString(), maxWords);
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static string? Sanitize(string? text, int maxWords)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > maxWords)
            {
                text = string.Join(" ", words.Take(maxWords));
            }

            return text.Trim();
        }
    }
}