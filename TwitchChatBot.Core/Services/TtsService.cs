using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class TtsService : ITtsService, IDisposable
    {
        private readonly ILogger<TtsService> _logger;
        private readonly IAlertService _alertService;
        private readonly string _ttsOutputDir;
        private readonly string _coquiExe; // Optional: path to custom run script or python command
        private readonly string _coquiModel;
        private readonly List<string> _generatedFiles = new();
        private bool _disposed;
        private const string DefaultSpeaker = "p225";

        public TtsService(ILogger<TtsService> logger, IAlertService alertService)
        {
            _logger = logger;
            _alertService = alertService;

            _ttsOutputDir = Path.Combine(AppSettings.Media.TwitchAlertsFolder!, "text_to_speach");
            Directory.CreateDirectory(_ttsOutputDir);

            _coquiExe = AppSettings.TTS.TtsExecutable;
            _coquiModel = AppSettings.TTS.DefaultModel;
        }

        public async Task SpeakAsync(string text, string? speakerOverride = null, string ? modelOverride = null)
        {
            var safeTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"tts_{safeTimestamp}.wav";
            var outputPath = Path.Combine(_ttsOutputDir, filename);
            var publicPath = $"/media/text_to_speach/{filename}";

            var model = string.IsNullOrWhiteSpace(modelOverride) ? _coquiModel : modelOverride;
            var voice = string.IsNullOrWhiteSpace(speakerOverride) ? DefaultSpeaker : speakerOverride;

            var psi = new ProcessStartInfo
            {
                FileName = _coquiExe,
                Arguments = $"--text \"{text.Replace("\"", "\\\"")}\" --model_name {model} --out_path \"{outputPath}\" --speaker_idx {voice} --use_cuda false",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            _logger.LogInformation("🔊 Running TTS: {Args}", psi.Arguments);

            try
            {
                using var proc = Process.Start(psi);
                if (proc == null)
                {
                    _logger.LogError("❌ Failed to start Coqui TTS process.");
                    return;
                }

                await proc.WaitForExitAsync();

                var errors = await proc.StandardError.ReadToEndAsync();
                if (!string.IsNullOrEmpty(errors))
                {
                    _logger.LogWarning("⚠️ Coqui stderr: {Errors}", errors);
                }

                if (File.Exists(outputPath))
                {
                    _generatedFiles.Add(outputPath);
                    _alertService.EnqueueAlert("", publicPath);
                }
                else
                {
                    _logger.LogWarning("❌ TTS output file not found: {Path}", outputPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error running Coqui TTS");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                foreach (var file in _generatedFiles)
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                _logger.LogInformation("🧹 TTS temp files deleted on shutdown.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ Failed to clean up TTS files.");
            }

            _disposed = true;
        }
    }
}