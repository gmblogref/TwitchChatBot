using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using TwitchChatBot.Models;

namespace TwitchChatBot.Data.Utilities
{
    public static class DataHelperMethods
    {
        public static async Task<T> LoadAsync<T>(string filePath, ILogger logger, string contextDescription, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Could not find {contextDescription} at path: {filePath}");
                }

                var json = await File.ReadAllTextAsync(filePath, cancellationToken);
                var result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize {contextDescription}.");
                }

                logger.LogInformation("📂 {Context} loaded successfully.", contextDescription);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to load {Context}.", contextDescription);
                throw;
            }
        }

        public static async Task<T> LoadOrCreateAsync<T>(string filePath, ILogger logger, string contextDescription, Func<T>? factory = null, CancellationToken cancellationToken = default)
            where T : class, new()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    logger.LogInformation("🆕 {Context} not found. Creating new at {Path}", contextDescription, filePath);
                    var created = factory != null ? factory() : new T();
                    return created;
                }

                var json = await File.ReadAllTextAsync(filePath, cancellationToken);
                var result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    logger.LogWarning("⚠️ {Context} was empty/invalid. Creating new.", contextDescription);
                    return factory != null ? factory() : new T();
                }

                logger.LogInformation("📂 {Context} loaded successfully.", contextDescription);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to load {Context}. Creating new.", contextDescription);
                return factory != null ? factory() : new T();
            }
        }

        public static async Task SaveAsync<T>(string filePath, T data, ILogger logger, string contextDescription, CancellationToken cancellationToken = default)
        {
            try
            {
                if (data == null)
                {
                    throw new ArgumentNullException(nameof(data));
                }

                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(data, options);

                var tempPath = filePath + ".tmp";
                await File.WriteAllTextAsync(tempPath, json, cancellationToken);

                File.Copy(tempPath, filePath, overwrite: true);
                File.Delete(tempPath);

                logger.LogInformation("💾 {Context} saved successfully.", contextDescription);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to save {Context}.", contextDescription);
                throw;
            }
        }

        public static string GetTwitchAlertMediaPath()
            => Path.GetFullPath(AppSettings.MediaMapFiles.TwitchAlertMedia!);

        public static string GetExcludedUsersMediaPath()
            => Path.GetFullPath(AppSettings.MediaMapFiles.ExcludedUsersMedia!);

        public static string GetFirstChattersMediaPath()
            => Path.GetFullPath(AppSettings.MediaMapFiles.FirstChattersMedia!);

        public static string GetCommandAlertMediaPath()
            => Path.GetFullPath(AppSettings.MediaMapFiles.CommandAlertMedia!);

        public static string GetUserWatchStreakMediaPath()
            => Path.GetFullPath(AppSettings.MediaMapFiles.UserWatchStreakMedia!);
    }
}
