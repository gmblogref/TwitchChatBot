# Testing and Manual Acceptance Guide

This guide documents the current automated-test project, safe commands, manual acceptance boundaries, CI workflows, and live-service risks. It does not claim that the commands were run for this documentation story.

Run commands from the repository root on Windows unless a section says otherwise. The repository-root `global.json` selects .NET SDK `10.0.402` with `latestPatch` roll-forward and prerelease SDKs disabled.

## Automated Test Stack

`TwitchChatBot.Tests` is the only automated test project. `TwitchChatBot.Testing` is a runtime utility used by the WinForms Testing tab and is **not** an isolated automated-test project.

Package versions in `TwitchChatBot.Tests.csproj` are:

| Package | Version | Purpose |
| --- | --- | --- |
| `xunit` | 2.9.3 | Test framework |
| `xunit.runner.visualstudio` | 3.1.5 | Visual Studio/test-platform adapter |
| `Microsoft.NET.Test.Sdk` | 18.11.0 | `dotnet test` and test-host integration |
| `Moq` | 4.20.72 | Mocks and interaction verification |
| `FluentAssertions` | 8.9.0 | Readable assertions |
| `coverlet.collector` | 10.0.0 | Optional coverage collection |

The project contains 164 `[Fact]` methods and no `[Theory]` methods at the time of this source review. This is a static inventory, not a test-execution result. Assembly-level configuration disables parallel execution because tests share static `AppSettings.Configuration` and other process-level state.

## Organization and Conventions

Tests are grouped under:

- `Core/`: selected business services, alert behavior, Twitch event routing, Watch Streaks, TTS, channel-point logic, roles, commands, and Wheel behavior.
- `Data/`: selected JSON repository lookup, caching, normalization, and persistence behavior.
- `Models/`: application-setting validation and example-template safety.

Repository rules require xUnit, Moq, FluentAssertions, Arrange/Act/Assert, one behavior per test, and names shaped as `MethodName_Should_DoExpectedBehavior_When_Scenario`. Existing older tests do not all follow that exact underscore pattern; new work should follow the current `AGENTS.md` rule rather than normalizing unrelated tests.

## Commands

Restore the solution:

```powershell
dotnet restore TwitchChatBot.sln
```

Build Debug after restore:

```powershell
dotnet build TwitchChatBot.sln --configuration Debug --no-restore
```

Build Release after restore:

```powershell
dotnet build TwitchChatBot.sln --configuration Release --no-restore
```

Run all unit tests after a Release build:

```powershell
dotnet test TwitchChatBot.Tests\TwitchChatBot.Tests.csproj --configuration Release --no-build
```

Run one test class:

```powershell
dotnet test TwitchChatBot.Tests\TwitchChatBot.Tests.csproj --configuration Release --no-build --filter "FullyQualifiedName~TwitchChatBot.Tests.Core.WheelServiceTests"
```

Run one test method:

```powershell
dotnet test TwitchChatBot.Tests\TwitchChatBot.Tests.csproj --configuration Release --no-build --filter "FullyQualifiedName=TwitchChatBot.Tests.Core.WheelServiceTests.SpinAsync_Should_Return_Item_When_Items_Exist"
```

Launch the Release application after a successful Release build and local configuration setup:

```powershell
.\TwitchChatBot\bin\Release\net10.0-windows\TwitchChatBot.UI.exe
```

The executable loads configuration from its own directory. Local build rules copy private `appsettings.json` and optional `appsettings.Development.json` there when they exist in the UI project directory. Never distribute that output without inspecting it for secrets.

## Verified Automated Coverage

Current automated tests exercise important behavior in these areas:

- Required-setting validation and placeholder safety.
- Alert enqueue order, disconnected-client behavior, completion, and retry handling.
- AI response parsing, prompt construction, HTTP failures, and exception fallback through a fake HTTP handler.
- Command lookup and command-service dispatch with mocked downstream services.
- excluded-user and First Chatter lookup/service behavior.
- Twitch alert handlers and selected TwitchClient event-routing behavior.
- Twitch role lookup delegation.
- Watch Streak state/service rules.
- channel-point lock and redemption orchestration behavior.
- Wheel CRUD, weighting, hidden entries, concurrency, shuffle, deterministic random selection, and dispatch fallbacks.
- selected repository caching/loading behavior.
- selected TTS validation and failure-tolerance behavior.

Important gaps include:

- No automated project reference to `TwitchChatBot.UI` or `TwitchChatBot.Testing`.
- No automated WinForms UI, designer, Wheel animation, result-panel, or button-event tests.
- No automated tests for `Program.Main`, dependency composition, `ChatBotController`, or full start/stop/shutdown order.
- No automated embedded ASP.NET Core host, browser JavaScript, real WebSocket, OBS, EventSub socket, or Streamlabs Socket.IO tests.
- No live Twitch, Helix, AWS Polly, OpenAI, or Streamlabs acceptance tests in the suite.
- Several Core services lack dedicated test classes, including donation alerts, alert history/replay, Helix lookup, moderation/Nuke, IRC notice handling, WebSocket server behavior, and gift-bundle suppression as an isolated service.
- Coverlet is installed, but the workflows do not publish coverage or enforce a coverage threshold.

Do not describe the suite as comprehensive. Historical passed-test counts belong in [Feature History](../FEATURE_HISTORY.md) only when backed by developer or CI evidence.

## Isolation and Known Automated-Test Side Effects

Most Core tests replace repositories, network boundaries, loggers, and downstream services with Moq objects. `AiTextServiceTests` use a fake `HttpMessageHandler`. Many repository tests inject in-memory configuration and seed private fields rather than reading private runtime data.

Isolation is not complete:

- `AppSettings.Configuration` is global static state. Parallel test execution is disabled, but not every test class restores the prior configuration.
- `AppSettingsTests` searches upward from the test output directory and reads the tracked `TwitchChatBot/appsettings.example.json`.
- `WheelRepositoryTests.SaveAllAsync_Should_SetCurrentState` configures the relative path `wheel-media.json`; it can create or replace that file in the test process working directory and does not delete it.
- Repository saves use a GUID-suffixed temporary file before moving/replacing the destination, but the tests do not generally allocate a unique temporary directory per test. The Wheel repository test therefore remains tied to its process working directory.
- Constructing `TtsService` creates an `alerts/text_to_speech` directory relative to the test process working directory and starts a background runner.
- The TTS runner performs AWS credential-chain discovery. If valid credentials are available before a `SpeakAsync` call, TTS tests can enqueue real Polly work, write MP3 files, and potentially incur AWS charges; the tests do not inject an `IAmazonPolly` client.
- `TtsServiceTests` do not dispose each constructed service, so its background runner and created files are not deterministically cleaned during the test method.

Run tests from a controlled working tree, inspect for generated `wheel-media.json` and `alerts/text_to_speech`, and avoid exposing chargeable AWS credentials when running the suite unless that risk is intentional. Do not point test configuration at production JSON files.

## Testing Tab Is a Production-Path Utility

The Testing tab resolves `TestUtilityService` from the same application service provider and calls production singleton services. It is not a sandbox.

The following source detail is especially important: `TriggerFirstChatTestAsync` passes `AppSettings.Twitch.TWITCH_BOT_ID` as the simulated chatter's user ID while using the entered name for username/display name. This can match or update a First Chatter mapping associated with the configured bot ID.

Testing actions may:

- send real Twitch chat, including command text and First Chatter fallback messages;
- invoke configured special commands, moderation, Nuke, or shout-out paths;
- mutate in-memory alert history and feature state;
- update and save First Chatter identity data;
- read or modify configured runtime JSON through production repositories;
- enqueue alerts to connected browser/OBS clients;
- call Twitch Helix endpoints;
- call OpenAI for qualifying generated-text paths;
- discover AWS credentials, call Polly, create MP3 files, and incur charges.

Test Bot mode suppresses Watch Streak statistics in several paths, but it does not isolate all production services. Back up mutable JSON, confirm the configured channel/accounts, use controlled data, and understand external costs before pressing Testing-tab buttons.

## Manual Acceptance Preparation

Before any live check:

1. Use a developer-controlled channel and identities. Do not experiment against an unrelated live channel.
2. Back up First Chatter, Watch Streak, and Wheel JSON files.
3. Verify which credentials and paid provider accounts are active.
4. Use sanitized media and confirm every mapped file exists beneath the configured media root.
5. Open application logs and keep `/alerts` open in a browser before testing visual alerts.
6. Add OBS only after browser behavior is understood.
7. Prefer one feature at a time and record observed logs/results without exposing tokens or private paths.

Detailed prerequisites are in the [Twitch](TWITCH_SETUP.md), [media](MEDIA_ENVIRONMENT_SETUP.md), [Streamlabs](STREAMLABS_SETUP.md), [OpenAI](OPENAI_SETUP.md), and [Polly](AWS_POLLY_SETUP.md) guides.

## Safe Manual Acceptance Matrix

| Area | Controlled procedure | Pass evidence | Principal risk |
| --- | --- | --- | --- |
| Startup/shutdown | Launch Release, inspect configuration validation, press Start once, and wait for IRC joined, EventSub welcome/subscription, and optional-provider logs. Press Stop, verify Watch Streak flush/stop-attempt logs, and observe connection logs before closing the form. Test a later Start only as a separate targeted scenario. | Local host binds and expected external stages report readiness. Stop records its state-flush and shutdown attempts; do not treat that alone as proof of permanent disconnection or restart readiness. | A returned Start call does not prove background providers are ready. Twitch reconnect/watchdog timers are not stopped by `DisconnectAsync`, background activity can end gradually, and the developer has observed unreliable Stop/Start reuse. Live Start can also persist a new Watch Streak index. |
| Twitch chat | Send one harmless message from a controlled nonexcluded account | Message appears in the WinForms chat view | First Chatter and auto-shout-out processing can also run |
| Commands | Use a harmless mapped command with no moderation/special behavior | Expected chat response and/or alert occurs once | Real chat, moderation, TTS, or persistent service behavior depending on configuration |
| Alerts/media | Connect `/alerts`; trigger one known mapping whose file exists | Browser acknowledges, displays/plays, and completes the alert | Missing media drops the entire alert; Polly or AI may also run for some categories |
| OBS | Add the same `/alerts` URL as an OBS Browser Source and trigger a controlled alert | OBS shows the same alert and application logs acknowledgement/completion | Browser-source refresh disconnects temporarily; black background is current CSS behavior |
| Wheel Spinner | Back up Wheel JSON, select a wheel, spin, verify displayed result, then press **OK** | Weighted selection animates, result panel appears, dispatch happens only after OK | UI edits persist; mapped command/channel-point dispatch can invoke production services |
| Channel points | Redeem a dedicated low-risk reward from a controlled account | EventSub add notification reaches the expected allowed/blocked alert path | Spends real points; blocked rewards can send chat and attempt cancellation/refund through Helix |
| First Chatter | Back up its map, use a controlled account not yet seen in this process, and send a message | One media or fallback welcome path occurs; repeats do not retrigger in the same session | Can send real chat and persist ID/username backfills; Testing tab uses the configured bot ID |
| Watch Streaks | Back up state, verify live versus Test Bot mode, then join/send the expected controlled event | Live mode records attendance/streak and flushes on stop; Test Bot mode avoids statistic updates | Live Start increments stream index and persistence is debounced; raw USERNOTICE handling requires a real Twitch event |
| Donations | Enable Streamlabs only in a controlled provider account and send a small provider test/controlled donation | Donation payload selects expected tier/default, alert appears, and speech is requested | External money/provider event, Streamlabs token exposure, Polly charges |
| OpenAI | Enable a constrained API key/budget and trigger one qualifying event | Generated line is used, or documented fallback occurs on failure | Sends event context to OpenAI and incurs usage charges |
| Polly | Use a permitted voice and short controlled phrase | MP3 is created, delivered through `/media`, played, and removed at shutdown best-effort | AWS charges and local file writes |

Do not use the Testing tab as proof that real EventSub registration, live Streamlabs delivery, OBS codec behavior, or Twitch authorization is correct; those require their controlled end-to-end paths.

Stop/Start reliability requires its own manual validation. Record whether Twitch disconnects, whether reconnect/watchdog logs continue after Stop, whether EventSub and Streamlabs listener tasks finish, whether the local overlay host stops, and whether a later Start restores each integration. Do not generalize one successful or failed provider result to every integration. Closing the form invokes full shutdown and ends the process; it is the current reliable operational way to end the session, not an equivalent test of an in-process Stop/Start cycle.

## CI and Version Policy

Two workflows are tracked:

- **Build** (`.github/workflows/build.yml`): on pull requests targeting `main`, checks out with `actions/checkout@v7`, selects the SDK from `global.json` through `actions/setup-dotnet@v6`, restores, and builds Release with `--no-restore`. It also declares a push trigger for `tasks/**`.
- **Version bump check** (`.github/workflows/version-check.yml`): on pull requests targeting `main`, compares the UI project's numeric `major.minor.patch` version with `origin/main`. It requires a strictly greater version only when the pull request contains a `.cs` or `.csproj` change. Documentation-only and workflow-only changes skip the bump requirement.

The repository's documented implementation branches use the singular prefix `task/...`, while the Build workflow's push pattern is plural `tasks/**`. Therefore a push to `task/...` does not match that push trigger. The pull-request trigger targeting `main` still applies when such a branch opens a PR. This guide records the mismatch; Story 1.9 does not change the workflow.

`AGENTS.md` states that `main` is protected and that Build and Version bump check are required. The developer owns branch creation/switching, restore/build/test/application execution, staging, commits, pushes, pull requests, and merges. Contributors and Codex must not claim CI or test success without an authoritative result.

## Acceptance Record

For a developer-run validation, record:

- commit/branch under review;
- exact command or manual path;
- configuration mode without secret values;
- passed/failed/skipped counts for automated tests;
- which live integrations were intentionally exercised;
- warnings, charges, persistent data changes, and cleanup performed;
- CI check results and PR/merge state.

Do not infer live readiness from compilation, unit tests, or the Start button alone.

## Related Documentation

- [Architecture Guide](ARCHITECTURE.md)
- [README](../README.md)
- [Configuration Reference](CONFIGURATION.md)
- [Twitch Developer and OAuth Setup](TWITCH_SETUP.md)
- [Media, Mapping, Overlay, and Windows Environment Setup](MEDIA_ENVIRONMENT_SETUP.md)
- [Streamlabs Donation Setup](STREAMLABS_SETUP.md)
- [OpenAI Alert-Text Setup](OPENAI_SETUP.md)
- [AWS Polly Text-to-Speech Setup](AWS_POLLY_SETUP.md)
- [Feature History](../FEATURE_HISTORY.md)
