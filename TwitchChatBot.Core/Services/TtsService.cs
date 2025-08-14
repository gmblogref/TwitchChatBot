using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime.Credentials;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class TtsService : ITtsService, IDisposable
    {
        private readonly ILogger<TtsService> _logger;
        private readonly IAlertService _alertService;

        private readonly string _ttsOutputDir;
        private readonly string _defaultVoice;
        private readonly RegionEndpoint _region;
        private readonly int _maxChars;

        private readonly ConcurrentQueue<(string text, string? voice)> _queue = new();
        private readonly CancellationTokenSource _runnerCts = new();
        private CancellationTokenSource? _currentItemCts;
        private Task? _runnerTask;
        private volatile bool _disposed;

        private volatile bool _hasAwsCreds;
        private DateTime _lastCredCheckUtc = DateTime.MinValue;
        private static readonly TimeSpan CredRecheckInterval = TimeSpan.FromMinutes(10);

        // Allow‑list of Polly voices. Add any you want to permit.
        private static readonly HashSet<string> PollyVoices = new(StringComparer.OrdinalIgnoreCase)
        {
            "Matthew","Joanna","Brian","Emma","Justin","Kimberly",
            "Amy","Russell","Nicole","Olivia","Stephen",
            "Salli","Joey","Ivy","Kendra"
        };

        public TtsService(ILogger<TtsService> logger, IAlertService alertService)
        {
            _logger = logger;
            _alertService = alertService;

            _ttsOutputDir = Path.Combine(AppSettings.Media.TwitchAlertsFolder!, "text_to_speach");
            Directory.CreateDirectory(_ttsOutputDir);

            _defaultVoice = string.IsNullOrWhiteSpace(AppSettings.TTS.DefaultSpeaker)
                ? "Matthew"
                : AppSettings.TTS.DefaultSpeaker!;

            var regionName = string.IsNullOrWhiteSpace(AppSettings.TTS.PollyRegion)
                ? "us-east-1"
                : AppSettings.TTS.PollyRegion!;
            _region = RegionEndpoint.GetBySystemName(regionName);

            _maxChars = AppSettings.TTS.MaxChars > 0 ? AppSettings.TTS.MaxChars : 350;

            _runnerTask = Task.Run(RunAsync);
        }

        public async Task SpeakAsync(string text, string? speakerOverride = null, string? modelOverride = null)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            if (!_hasAwsCreds)
            {
                _logger.LogWarning("🛑 Skipping TTS enqueue: AWS credentials not available.");
                // opportunistic re-check if it's been a while
                if (DateTime.UtcNow - _lastCredCheckUtc > CredRecheckInterval)
                    await AwsCheckCredentialsAsync();
                return;
            }

            _queue.Enqueue((text, speakerOverride));
        }

        public void SkipCurrent()
        {
            try
            {
                _currentItemCts?.Cancel();
                _logger.LogInformation("⏭️ Polly TTS skip requested.");
            }
            catch { /* ignore */ }
        }

        public void ResetQueue()
        {
            try
            {
                while (_queue.TryDequeue(out _)) { }
                _currentItemCts?.Cancel();
                _logger.LogInformation("♻️ Polly TTS queue reset (cleared & canceled current).");
            }
            catch { /* ignore */ }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _runnerCts.Cancel();
                _currentItemCts?.Cancel();
            }
            catch { /* ignore */ }

            try
            {
                if (Directory.Exists(_ttsOutputDir))
                {
                    foreach (var file in Directory.EnumerateFiles(_ttsOutputDir, "*.*", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            File.Delete(file);
                            _logger.LogInformation("🧹 Deleted TTS file: {File}", file);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "⚠️ Failed to delete TTS file: {File}", file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ Failed to clean up TTS folder.");
            }
        }

        private async Task RunAsync()
        {
            // Log credentials status once at startup of the TTS runner
            await AwsCheckCredentialsAsync(); 
            
            using var polly = new AmazonPollyClient(_region);
            var token = _runnerCts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (DateTime.UtcNow - _lastCredCheckUtc > CredRecheckInterval)
                    {
                        await AwsCheckCredentialsAsync();
                    }

                    if (!_queue.TryDequeue(out var job))
                    {
                        await Task.Delay(50, token);
                        continue;
                    }

                    _currentItemCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                    try
                    {
                        await ProcessOneAsync(polly, job.text, job.voice, _currentItemCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("⏹️ Polly TTS item canceled.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Polly TTS item failed.");
                    }
                    finally
                    {
                        _currentItemCts.Dispose();
                        _currentItemCts = null;
                    }
                }
                catch (OperationCanceledException)
                {
                    // shutting down
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Polly TTS runner loop error; continuing.");
                    await Task.Delay(250, token);
                }
            }
        }

        private async Task ProcessOneAsync(AmazonPollyClient polly, string rawText, string? voiceOverride, CancellationToken ct)
        {
            var text = Sanitize(rawText, _maxChars);
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var voice = ResolveVoice(voiceOverride);
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            var mp3Path = Path.Combine(_ttsOutputDir, $"tts_{ts}.mp3");
            var publicPath = $"/media/text_to_speach/{Path.GetFileName(mp3Path)}";

            // Prefer Neural if supported; fallback to Standard automatically.
            var req = new SynthesizeSpeechRequest
            {
                Text = text,
                VoiceId = voice,
                OutputFormat = OutputFormat.Mp3,
                Engine = Engine.Neural
            };

            _logger.LogInformation("🔊 Polly synth: {Voice} (pref: Neural) \"{Preview}\"",
                voice, text.Length > 80 ? text[..80] + "…" : text);

            try
            {
                using var resp = await polly.SynthesizeSpeechAsync(req, ct);
                await using var fs = File.Create(mp3Path);
                await resp.AudioStream.CopyToAsync(fs, ct);
            }
            catch (AmazonPollyException ex) when (ex.Message.Contains("Neural", StringComparison.OrdinalIgnoreCase))
            {
                // Voice/region may not support Neural; retry with Standard.
                _logger.LogInformation("ℹ️ Voice {Voice} not Neural in {Region}. Retrying with Standard.", voice, _region.SystemName);
                req.Engine = Engine.Standard;
                using var resp = await polly.SynthesizeSpeechAsync(req, ct);
                await using var fs = File.Create(mp3Path);
                await resp.AudioStream.CopyToAsync(fs, ct);
            }

            _alertService.EnqueueAlert("", publicPath);
            _logger.LogInformation("✅ Polly TTS ready: {File}", mp3Path);
        }

        private string ResolveVoice(string? voiceOverride)
        {
            if (!string.IsNullOrWhiteSpace(voiceOverride) && PollyVoices.Contains(voiceOverride))
            {
                return voiceOverride;
            }

            return _defaultVoice;
        }

        private string Sanitize(string s, int maxChars)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            // Collapse excessive repeats (e.g., looooool -> loool) + trim + hard cap.
            var sb = new System.Text.StringBuilder(s.Length);
            int run = 0; char prev = '\0';
            foreach (var ch in s)
            {
                if (ch == prev) run++; else { prev = ch; run = 1; }
                if (run <= 4) sb.Append(ch);
            }
            var trimmed = sb.ToString().Trim();
            return trimmed.Length > maxChars ? trimmed[..maxChars] + "…" : trimmed;
        }

        private async Task AwsCheckCredentialsAsync()
        {
            try
            {
                var resolver = new DefaultAWSCredentialsIdentityResolver();
                var config = new AmazonPollyConfig { RegionEndpoint = _region };

                var identity = await resolver.ResolveIdentityAsync(config, CancellationToken.None);
                _lastCredCheckUtc = DateTime.UtcNow;

                if (identity == null)
                {
                    _hasAwsCreds = false;
                    _logger.LogWarning("⚠️ AWS credentials not found. Set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY env vars or use ~/.aws/credentials.");
                    return;
                }

                var immutableCreds = identity.GetCredentials(); // correct for your version
                if (string.IsNullOrWhiteSpace(immutableCreds.AccessKey) || string.IsNullOrWhiteSpace(immutableCreds.SecretKey))
                {
                    _hasAwsCreds = false;
                    _logger.LogWarning("⚠️ AWS credentials are configured but empty. Check AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY values.");
                    return;
                }

                _hasAwsCreds = true;
                _logger.LogInformation("✅ AWS credentials detected for AccessKey: {KeyId}", immutableCreds.AccessKey);
            }
            catch (Exception ex)
            {
                _hasAwsCreds = false;
                _lastCredCheckUtc = DateTime.UtcNow;
                _logger.LogError(ex, "❌ Failed to load AWS credentials. Polly TTS will not work until credentials are fixed.");
            }
        }
    }
}