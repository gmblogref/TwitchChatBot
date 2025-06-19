using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TwitchChatBot.Data.Utilities
{
    public static class DataHelperMethods
    {
        public static async Task<T> LoadAsync<T>(string filePath, ILogger logger, string contextDescription, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Could not find {contextDescription} at path: {filePath}");

                var json = await File.ReadAllTextAsync(filePath, cancellationToken);
                var result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                    throw new InvalidOperationException($"Failed to deserialize {contextDescription}.");

                logger.LogInformation("📂 {Context} loaded successfully.", contextDescription);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to load {Context}.", contextDescription);
                throw;
            }
        }
    }
}
