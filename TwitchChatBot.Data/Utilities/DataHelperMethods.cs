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
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                options.Converters.Add(new JsonStringEnumConverter());

                var result = JsonSerializer.Deserialize<T>(json, options);

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
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                options.Converters.Add(new JsonStringEnumConverter());

                var result = JsonSerializer.Deserialize<T>(json, options);

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
            string? tempPath = null;

            try
            {
                if (data == null)
                {
                    logger.LogWarning("⚠️ Save skipped because data is null. Context={Context}", contextDescription);
                    return;
                }

                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(data, options);

                tempPath = $"{filePath}.{Guid.NewGuid():N}.tmp";
                await File.WriteAllTextAsync(tempPath, json, cancellationToken);

                if (File.Exists(filePath))
                {
                    File.Replace(tempPath, filePath, destinationBackupFileName: null);
                }
                else
                {
                    File.Move(tempPath, filePath);
                }

                logger.LogInformation("💾 {Context} saved successfully.", contextDescription);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to save {Context}.", contextDescription);
                throw;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(tempPath) && File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                        // Best-effort cleanup only.
                    }
                }
            }
        }

        public static string GetTwitchAlertMediaPath()
            => Path.GetFullPath(AppSettings.MediaMapFiles.TwitchAlertMedia);

        public static string GetExcludedUsersMediaPath()
            => Path.GetFullPath(AppSettings.MediaMapFiles.ExcludedUsersMedia);

        public static string GetFirstChattersMediaPath()
            => Path.GetFullPath(AppSettings.MediaMapFiles.FirstChattersMedia);

        public static string GetCommandAlertMediaPath()
            => Path.GetFullPath(AppSettings.MediaMapFiles.CommandAlertMedia);

        public static string GetUserWatchStreakMediaPath()
            => Path.GetFullPath(AppSettings.MediaMapFiles.UserWatchStreakMedia);

		public static string GetWheelMediaPath()
			=> Path.GetFullPath(AppSettings.MediaMapFiles.WheelMedia);

		public static string GetDonationMediaPath()
			=> Path.GetFullPath(AppSettings.MediaMapFiles.DonationMedia);
	}
}
