using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime.Credentials;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Core.Utilities;
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

        private readonly ConcurrentQueue<(long sequence, string text, string? voice)> _queue = new();
		private readonly object _queueSync = new();
		private readonly object _currentItemSync = new();
		private readonly SemaphoreSlim _cancellationGate = new(1, 1);
        private readonly CancellationTokenSource _runnerCts = new();
        private CancellationTokenSource? _currentItemCts;
		private TaskCompletionSource<bool>? _currentItemCompletion;
		private long? _currentItemSequence;
		private long _lastTtsSequence;
        private Task? _runnerTask;
        private volatile bool _disposed;

        private volatile bool _hasAwsCreds;
        private DateTime _lastCredCheckUtc = DateTime.MinValue;
        private static readonly TimeSpan CredRecheckInterval = TimeSpan.FromMinutes(10);

        private static HashSet<string>? _pollyVoices;
        private static HashSet<string> PollyVoices
        {
            get
            {
                if (_pollyVoices == null)
                {
                    _pollyVoices = new HashSet<string>(
                        AppSettings.TTS.AllowedVoices,
                        StringComparer.OrdinalIgnoreCase);
                }

                if (_pollyVoices.Count == 0)
                {
                    _pollyVoices.Add("Matthew");
                }

                return _pollyVoices;
            }
        }

        public TtsService(ILogger<TtsService> logger, IAlertService alertService)
        {
            _logger = logger;
            _alertService = alertService;

            _ttsOutputDir = CoreHelperMethods.GetTtsOutputFolder();
            Directory.CreateDirectory(_ttsOutputDir);

            _defaultVoice = string.IsNullOrWhiteSpace(AppSettings.TTS.DefaultSpeaker)
                ? "Matthew"
                : AppSettings.TTS.DefaultSpeaker;

            var regionName = string.IsNullOrWhiteSpace(AppSettings.TTS.PollyRegion)
                ? "us-east-1"
                : AppSettings.TTS.PollyRegion;
            _region = RegionEndpoint.GetBySystemName(regionName);

            _maxChars = AppSettings.TTS.MaxChars > 0 ? AppSettings.TTS.MaxChars : 350;

            _runnerTask = Task.Run(RunAsync);
        }

        public async Task SpeakAsync(string text, string? speakerOverride = null, string? modelOverride = null)
        {
            if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}

            if (!_hasAwsCreds)
            {
                _logger.LogWarning("🛑 Skipping TTS enqueue: AWS credentials not available.");
                // opportunistic re-check if it's been a while
				if (DateTime.UtcNow - _lastCredCheckUtc > CredRecheckInterval)
				{
                    await AwsCheckCredentialsAsync();
				}

                return;
            }

			lock (_queueSync)
			{
				long sequence = ++_lastTtsSequence;
				_queue.Enqueue((sequence, text, speakerOverride));
			}
        }

        public void SkipCurrent()
        {
			_ = SkipCurrentAsync();
        }

        public void ResetQueue()
        {
			long throughSequence;
			long? currentSequence = null;
			Task? currentCompletion = null;

			lock (_queueSync)
			{
				throughSequence = _lastTtsSequence;

				while (_queue.TryDequeue(out _))
				{
				}

				lock (_currentItemSync)
				{
					if (_currentItemSequence.HasValue &&
						_currentItemSequence.Value <= throughSequence)
					{
						currentSequence = _currentItemSequence;
						currentCompletion = _currentItemCompletion?.Task;
						TryCancelCurrentItem();
					}
				}
			}

			_ = ResetQueueAsync(throughSequence, currentSequence, currentCompletion);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _runnerCts.Cancel();

				lock (_currentItemSync)
				{
					TryCancelCurrentItem();
				}
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

					(long sequence, string text, string? voice) job = default;
					CancellationTokenSource? itemCts = null;
					TaskCompletionSource<bool>? itemCompletion = null;

					lock (_queueSync)
					{
						if (_queue.TryDequeue(out job))
						{
							itemCts = CancellationTokenSource.CreateLinkedTokenSource(token);
							itemCompletion = new TaskCompletionSource<bool>(
								TaskCreationOptions.RunContinuationsAsynchronously);

							lock (_currentItemSync)
							{
								_currentItemCts = itemCts;
								_currentItemCompletion = itemCompletion;
								_currentItemSequence = job.sequence;
							}
						}
					}

					if (itemCts == null || itemCompletion == null)
                    {
                        await Task.Delay(50, token);
                        continue;
                    }

                    try
                    {
						await ProcessTextToSpeechAsync(
							polly,
							job.sequence,
							job.text,
							job.voice,
							itemCts.Token);
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
						lock (_currentItemSync)
						{
							if (ReferenceEquals(_currentItemCts, itemCts))
							{
								_currentItemCts = null;
								_currentItemCompletion = null;
								_currentItemSequence = null;
							}
						}

						itemCts.Dispose();
						itemCompletion.TrySetResult(true);
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

		private async Task ProcessTextToSpeechAsync(
			AmazonPollyClient polly,
			long ttsSequence,
			string rawText,
			string? voiceOverride,
			CancellationToken ct)
        {
            var text = Sanitize(rawText, _maxChars);
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var (voice, finalText) = ResolveVoiceAndText(voiceOverride, text);
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            var mp3Path = Path.Combine(_ttsOutputDir, $"tts_{ts}.mp3");
            var publicPath = $"/media/text_to_speech/{Path.GetFileName(mp3Path)}";
			bool alertEnqueued = false;

            // Prefer Neural if supported; fallback to Standard automatically.
            var req = new SynthesizeSpeechRequest
            {
                Text = finalText,
                VoiceId = voice,
                OutputFormat = OutputFormat.Mp3,
                Engine = Engine.Neural
            };

            _logger.LogInformation("🔊 Polly synth: {Voice} (pref: Neural) \"{Preview}\"",
                voice, finalText.Length > 80 ? finalText[..80] + "…" : finalText);

			try
			{
				try
				{
					using (var resp = await polly.SynthesizeSpeechAsync(req, ct))
					{
						await using (var fs = File.Create(mp3Path))
						{
							await resp.AudioStream.CopyToAsync(fs, ct);
							await fs.FlushAsync(ct);
						}
					}
				}
				catch (AmazonPollyException ex) when (ex.Message.Contains("Neural", StringComparison.OrdinalIgnoreCase))
				{
					_logger.LogInformation(
						"ℹ️ Voice {Voice} not Neural in {Region}. Retrying with Standard.",
						voice,
						_region.SystemName);

					req.Engine = Engine.Standard;

					using (var resp = await polly.SynthesizeSpeechAsync(req, ct))
					{
						await using (var fs = File.Create(mp3Path))
						{
							await resp.AudioStream.CopyToAsync(fs, ct);
							await fs.FlushAsync(ct);
						}
					}
				}

				await Task.Delay(100, ct);
				ct.ThrowIfCancellationRequested();

				// Log file size for diagnostics; can be useful to identify issues with Polly responses (e.g., very small files may indicate errors).
				var fileInfo = new FileInfo(mp3Path);
				_logger.LogInformation("TTS MP3 Size: {Bytes}", fileInfo.Length);

				_alertService.EnqueueAlert(new AlertItem
				{
					Type = "alert",
					Message = string.Empty,
					MediaPath = publicPath,
					IsTts = true,
					TtsSequence = ttsSequence
				});

				alertEnqueued = true;
				_logger.LogInformation("✅ Polly TTS ready: {File}", mp3Path);
			}
			finally
			{
				if (!alertEnqueued)
				{
					TryDeleteTtsFile(mp3Path);
				}
			}
        }

		private async Task SkipCurrentAsync()
		{
			await _cancellationGate.WaitAsync().ConfigureAwait(false);
			try
			{
				if (await _alertService.TryCancelActiveTtsAsync().ConfigureAwait(false))
				{
					_logger.LogInformation("⏭️ TTS skip stopped active overlay playback.");
					return;
				}

				long? currentSequence;
				Task? currentCompletion;

				lock (_currentItemSync)
				{
					currentSequence = _currentItemSequence;
					currentCompletion = _currentItemCompletion?.Task;

					if (currentSequence.HasValue)
					{
						TryCancelCurrentItem();
					}
				}

				if (!currentSequence.HasValue)
				{
					_logger.LogInformation("⏭️ TTS skip requested with no active speech.");
					return;
				}

				if (currentCompletion != null)
				{
					await currentCompletion.ConfigureAwait(false);
				}

				IReadOnlyList<string> mediaPaths = await _alertService
					.CancelTtsAsync(currentSequence.Value)
					.ConfigureAwait(false);

				DeleteTtsFiles(mediaPaths);
				_logger.LogInformation("⏭️ TTS skip canceled synthesis for item {Sequence}.", currentSequence.Value);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "TTS skip failed.");
			}
			finally
			{
				_cancellationGate.Release();
			}
		}

		private async Task ResetQueueAsync(
			long throughSequence,
			long? currentSequence,
			Task? currentCompletion)
		{
			await _cancellationGate.WaitAsync().ConfigureAwait(false);
			try
			{
				var mediaPaths = new List<string>();
				IReadOnlyList<string> resetPaths = await _alertService
					.ResetTtsAsync(throughSequence)
					.ConfigureAwait(false);

				mediaPaths.AddRange(resetPaths);

				if (currentCompletion != null)
				{
					await currentCompletion.ConfigureAwait(false);
				}

				if (currentSequence.HasValue)
				{
					IReadOnlyList<string> racedPaths = await _alertService
						.CancelTtsAsync(currentSequence.Value)
						.ConfigureAwait(false);

					mediaPaths.AddRange(racedPaths);
				}

				DeleteTtsFiles(mediaPaths);
				_logger.LogInformation(
					"♻️ TTS reset completed through item {Sequence}; future speech remains available.",
					throughSequence);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "TTS reset failed.");
			}
			finally
			{
				_cancellationGate.Release();
			}
		}

		private void TryCancelCurrentItem()
		{
			try
			{
				_currentItemCts?.Cancel();
			}
			catch (ObjectDisposedException)
			{
				// The runner completed the item while cancellation was being requested.
			}
		}

		private void DeleteTtsFiles(IEnumerable<string> mediaPaths)
		{
			foreach (string mediaPath in mediaPaths.Distinct(StringComparer.OrdinalIgnoreCase))
			{
				string absolutePath = CoreHelperMethods.ToAbsoluteMediaPath(mediaPath);
				TryDeleteTtsFile(absolutePath);
			}
		}

		private void TryDeleteTtsFile(string path)
		{
			try
			{
				if (File.Exists(path))
				{
					File.Delete(path);
					_logger.LogInformation("🧹 Deleted canceled TTS file: {File}", path);
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to delete canceled TTS file: {File}", path);
			}
		}

        private (string voice, string text) ResolveVoiceAndText(string? voiceOverride, string text)
        {
            if (!string.IsNullOrWhiteSpace(voiceOverride))
            {
                if (PollyVoices.Contains(voiceOverride))
                {
                    return (voiceOverride, text); // ✅ Valid override
                }

                // ❌ Invalid override — prepend to text
                text = $"{voiceOverride} {text}";
            }

            return (_defaultVoice, text); // Use default voice
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
                _logger.LogInformation("✅ AWS credentials detected.");
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
