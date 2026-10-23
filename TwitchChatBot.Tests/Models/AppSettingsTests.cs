using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using TwitchChatBot.Models;
using Xunit;

namespace TwitchChatBot.Tests.Models
{
    public class AppSettingsTests : IDisposable
    {
        private readonly IConfiguration? _originalConfiguration;

        public AppSettingsTests()
        {
            _originalConfiguration = AppSettings.Configuration;
        }

        public void Dispose()
        {
            AppSettings.Configuration = _originalConfiguration;
        }

        [Fact]
        public void ValidateRequiredSettings_Should_ReportMissingKey_When_RequiredSettingIsAbsent()
        {
            // Arrange
            Dictionary<string, string?> settings = LoadExampleSettings();
            settings.Remove("Twitch:Channel");
            IConfiguration configuration = CreateConfiguration(settings);

            // Act
            IReadOnlyList<string> errors = AppSettings.ValidateRequiredSettings(configuration);

            // Assert
            errors.Should().Contain("Missing required configuration key: 'Twitch:Channel'.");
        }

        [Fact]
        public void ValidateRequiredSettings_Should_ReportMalformedInteger_When_IntegerIsInvalid()
        {
            // Arrange
            Dictionary<string, string?> settings = LoadExampleSettings();
            settings["Ads:AdIntervalMinutes"] = "not-an-integer";
            IConfiguration configuration = CreateConfiguration(settings);

            // Act
            IReadOnlyList<string> errors = AppSettings.ValidateRequiredSettings(configuration);

            // Assert
            errors.Should().Contain("Configuration key 'Ads:AdIntervalMinutes' must be a valid integer.");
        }

        [Fact]
        public void ValidateRequiredSettings_Should_NotIncludeConfigurationValues_When_ValidationFails()
        {
            // Arrange
            const string credentialValue = "private-access-token-value";
            const string invalidValue = "private-invalid-value";
            Dictionary<string, string?> settings = LoadExampleSettings();
            settings["Auth:AccessToken"] = credentialValue;
            settings["Ads:AdIntervalMinutes"] = invalidValue;
            IConfiguration configuration = CreateConfiguration(settings);

            // Act
            IReadOnlyList<string> errors = AppSettings.ValidateRequiredSettings(configuration);

            // Assert
            string.Join(Environment.NewLine, errors).Should().NotContain(credentialValue).And.NotContain(invalidValue);
        }

        [Fact]
        public void GetOptionalStringSetting_Should_ReturnNull_When_SettingIsMissing()
        {
            // Arrange
            AppSettings.Configuration = CreateConfiguration(new Dictionary<string, string?>());

            // Act
            string? result = AppSettings.GetOptionalStringSetting("Optional:Missing");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetOptionalStringSetting_Should_ReturnNull_When_SettingIsBlank()
        {
            // Arrange
            AppSettings.Configuration = CreateConfiguration(
                new Dictionary<string, string?>
                {
                    ["Optional:Blank"] = "   "
                });

            // Act
            string? result = AppSettings.GetOptionalStringSetting("Optional:Blank");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void WheelCenterBranding_Should_ReturnNull_When_SettingIsMissing()
        {
            // Arrange
            AppSettings.Configuration = CreateConfiguration(new Dictionary<string, string?>());

            // Act
            string? result = AppSettings.UiMedia.WheelCenterBranding;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void WheelCenterBranding_Should_ReturnConfiguredPath_When_SettingIsPresent()
        {
            // Arrange
            AppSettings.Configuration = CreateConfiguration(
                new Dictionary<string, string?>
                {
                    ["UiMedia:WheelCenterBranding"] = "Wheel/branding.png"
                });

            // Act
            string? result = AppSettings.UiMedia.WheelCenterBranding;

            // Assert
            result.Should().Be("Wheel/branding.png");
        }

        [Fact]
        public void GetIntSetting_Should_ThrowFormatException_When_IntegerIsMalformed()
        {
            // Arrange
            const string invalidValue = "sensitive-invalid-value";
            AppSettings.Configuration = CreateConfiguration(
                new Dictionary<string, string?>
                {
                    ["Ads:AdInitialSeconds"] = invalidValue
                });

            // Act
            Action action = () => AppSettings.GetIntSetting("Ads:AdInitialSeconds");

            // Assert
            FormatException exception = action.Should().Throw<FormatException>().Which;
            exception.Message.Should().Contain("Ads:AdInitialSeconds").And.NotContain(invalidValue);
        }

        [Fact]
        public void IsConfigured_Should_ReturnFalse_When_StreamlabsSettingsAreMissing()
        {
            // Arrange
            AppSettings.Configuration = CreateConfiguration(new Dictionary<string, string?>());

            // Act
            bool result = AppSettings.Streamlabs.IsConfigured;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsConfigured_Should_ReturnTrue_When_StreamlabsSettingsArePresent()
        {
            // Arrange
            AppSettings.Configuration = CreateConfiguration(
                new Dictionary<string, string?>
                {
                    ["Streamlabs:SocketUrl"] = "https://streamlabs.example.invalid",
                    ["Streamlabs:SocketToken"] = "placeholder-token"
                });

            // Act
            bool result = AppSettings.Streamlabs.IsConfigured;

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateRequiredSettings_Should_ReportWheelMediaKey_When_WheelMediaIsAbsent()
        {
            // Arrange
            Dictionary<string, string?> settings = LoadExampleSettings();
            settings.Remove("MediaMapFiles:WheelMedia");
            IConfiguration configuration = CreateConfiguration(settings);

            // Act
            IReadOnlyList<string> errors = AppSettings.ValidateRequiredSettings(configuration);

            // Assert
            errors.Should().Contain("Missing required configuration key: 'MediaMapFiles:WheelMedia'.");
        }

        [Fact]
        public void ValidateRequiredSettings_Should_ReportDonationMediaKey_When_DonationMediaIsAbsent()
        {
            // Arrange
            Dictionary<string, string?> settings = LoadExampleSettings();
            settings.Remove("MediaMapFiles:DonationMedia");
            IConfiguration configuration = CreateConfiguration(settings);

            // Act
            IReadOnlyList<string> errors = AppSettings.ValidateRequiredSettings(configuration);

            // Assert
            errors.Should().Contain("Missing required configuration key: 'MediaMapFiles:DonationMedia'.");
        }

        [Fact]
        public void LoadExampleSettings_Should_ContainRequiredKeys_When_TemplateIsRead()
        {
            // Arrange
            string[] requiredKeys =
            {
                "MediaMapFiles:WheelMedia",
                "MediaMapFiles:DonationMedia",
                "Streamlabs:SocketUrl",
                "Streamlabs:SocketToken"
            };

            // Act
            Dictionary<string, string?> settings = LoadExampleSettings();

            // Assert
            settings.Keys.Should().Contain(requiredKeys);
        }

        [Fact]
        public void LoadExampleSettings_Should_ContainWheelCenterBrandingKey_When_TemplateIsRead()
        {
            // Arrange
            Dictionary<string, string?> settings = LoadExampleSettings();

            // Act
            bool containsKey = settings.ContainsKey("UiMedia:WheelCenterBranding");

            // Assert
            containsKey.Should().BeTrue();
        }

        [Fact]
        public void ValidateRequiredSettings_Should_ReturnNoErrors_When_ExampleTemplateIsLoaded()
        {
            // Arrange
            IConfiguration configuration = CreateConfiguration(LoadExampleSettings());

            // Act
            IReadOnlyList<string> errors = AppSettings.ValidateRequiredSettings(configuration);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void LoadExampleSettings_Should_ContainOnlyPlaceholders_When_SecretSettingsAreRead()
        {
            // Arrange
            string[] secretKeys =
            {
                "Auth:BotClientSecret",
                "Auth:OAuthToken",
                "Auth:OAuthBearerBot",
                "Auth:AccessToken",
                "Auth:AppAccessToken",
                "Auth:RefreshToken",
                "EventSub:Secret",
                "Streamlabs:SocketToken",
                "OpenAI:ApiKey"
            };

            // Act
            Dictionary<string, string?> settings = LoadExampleSettings();

            // Assert
            foreach (string key in secretKeys)
            {
                string? value = settings[key];
                bool isSafePlaceholder = string.IsNullOrWhiteSpace(value) ||
                    value.StartsWith("YOUR_", StringComparison.Ordinal);
                isSafePlaceholder.Should().BeTrue($"{key} must be blank or use a YOUR_ placeholder");
            }
        }

        private static IConfiguration CreateConfiguration(Dictionary<string, string?> settings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        private static Dictionary<string, string?> LoadExampleSettings()
        {
            string path = FindExampleSettingsPath();
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            var settings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            AddConfigurationValues(settings, string.Empty, document.RootElement);
            return settings;
        }

        private static string FindExampleSettingsPath()
        {
            DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "TwitchChatBot", "appsettings.example.json");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not locate TwitchChatBot/appsettings.example.json.");
        }

        private static void AddConfigurationValues(
            Dictionary<string, string?> settings,
            string prefix,
            JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string key = string.IsNullOrEmpty(prefix)
                        ? property.Name
                        : $"{prefix}:{property.Name}";
                    AddConfigurationValues(settings, key, property.Value);
                }

                return;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                int index = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    AddConfigurationValues(settings, $"{prefix}:{index}", item);
                    index++;
                }

                return;
            }

            settings[prefix] = element.ValueKind == JsonValueKind.String
                ? element.GetString()
                : element.GetRawText();
        }
    }
}
