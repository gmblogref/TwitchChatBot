# Feature History

This file records implemented behavior and completed story outcomes. It is not the future-feature backlog. Statuses mean:

- **COMPLETE**: implementation is accepted or has explicit developer/merged-PR validation.
- **IMPLEMENTED — Pending Validation**: implementation exists, but a required manual or live validation gap remains.
- **IN PROGRESS**: approved work is being implemented or awaits developer acceptance.
- **PLANNED / BACKLOG**: retained only when needed to distinguish a known future change from current behavior.

## Foundational Runtime Capabilities

### Twitch Chat Integration

- **Status:** COMPLETE
- The application uses `TwitchClientWrapper` for Twitch chat connectivity, incoming message handling, viewer updates, outgoing messages, and reconnect/watchdog behavior.
- Developer validation for Story 1.2.1 confirmed incoming Twitch chat and outgoing chat through the First Chatter response.
- **Planned distinction:** migration of chat read/write behavior to the Twitch Chat API is PLANNED / BACKLOG; the current TwitchLib-based behavior remains implemented.
- **Evidence:** `TwitchChatBot.Core/Services/TwitchClientWrapper.cs`, `TwitchChatBot.Tests/Core/TwitchClientWrapperTests.cs`.

### EventSub WebSocket Handling

- **Status:** IMPLEMENTED — Pending Validation
- `EventSubSocketService` maintains the EventSub WebSocket lifecycle, subscription setup, reconnect handling, and routing of supported events into alert and channel-point services.
- Story 1.2.1 runtime validation did not exercise every live EventSub event type.
- **Evidence:** `TwitchChatBot/Services/EventSubSocketService.cs` and its registered service contracts and handlers.

### Alert Processing and Media Playback

- **Status:** COMPLETE
- Alerts are queued, delivered to browser clients through the embedded ASP.NET Core/WebSocket host, acknowledged, retried within configured limits, and mapped to public media paths.
- Story 1.2.1 developer validation confirmed browser alert display and OBS alert display after refreshing the OBS browser source.
- **Evidence:** `TwitchChatBot.Core/Services/AlertService.cs`, `TwitchChatBot.Core/Services/WebSocketServer.cs`, `TwitchChatBot/WebHostWrapper.cs`, and `TwitchChatBot/wwwroot/alerts.html`.

### AWS Polly Text-to-Speech

- **Status:** COMPLETE
- `TtsService` queues speech requests, resolves configured voices, uses AWS Polly, writes temporary MP3 output under the media directory, and falls back from neural to standard synthesis when appropriate.
- Story 1.2.1 developer validation confirmed AWS Polly output through the follow test alert.
- Validation was not exhaustive across every live event type, and future live Polly usage may incur AWS charges.
- **Evidence:** `TwitchChatBot.Core/Services/TtsService.cs`, `TwitchChatBot.Tests/Core/TtsServiceTests.cs`.

### Chat Commands

- **Status:** IMPLEMENTED — Pending Validation
- `CommandAlertService` parses configured commands, resolves command media, supports special command behavior, and integrates with alert, moderation, role, TTS, and First Chatter services.
- Unit-test coverage exists, but no separate historical live-command acceptance report was located during Story 1.3 review.
- **Evidence:** `TwitchChatBot.Core/Services/CommandAlertService.cs`, `TwitchChatBot.Tests/Core/CommandAlertServiceTests.cs`.

### First Chatter Detection

- **Status:** COMPLETE
- First-chat participation is tracked per stream, excluded users are filtered, configured media can play, and fallback chat responses are supported.
- Story 1.2.1 developer validation confirmed the outgoing First Chatter response.
- Testing Tab isolation remains pending work and is not part of the accepted runtime behavior.
- **Evidence:** `TwitchChatBot.Core/Services/FirstChatterAlertService.cs`, `TwitchChatBot.Data/FirstChatterMediaRepository.cs`, and their unit tests.

### Watch Streak Tracking

- **Status:** IMPLEMENTED — Pending Validation
- Watch streak state tracks stream attendance, current and longest streaks, debounced persistence, and testing/replay safeguards.
- Unit-test coverage exists, but no separate historical live acceptance report was located during Story 1.3 review.
- **Evidence:** `TwitchChatBot.Core/Services/WatchStreakService.cs`, `TwitchChatBot.Data/WatchStreakRepository.cs`, and their unit tests.

## Implemented Feature Milestones

### Wheel Spinner System — PRs #88 and #89

- **Status:** COMPLETE
- Added JSON-backed wheel storage, weighted selection, item ordering and visibility, shuffle behavior, wheel management, winner animation, and alert/command triggering from wheel results.
- Random selection is abstracted through `IRandomProvider` for targeted unit testing.
- The WinForms wheel tab and `WheelSpinnerControl` provide the runtime UI.
- **Evidence:** [PR #88](https://github.com/gmblogref/TwitchChatBot/pull/88), [PR #89](https://github.com/gmblogref/TwitchChatBot/pull/89), `WheelService`, `WheelRepository`, `WheelSpinnerControl`, and their tests.

### Streamlabs Donation Integration — PR #93

- **Status:** IMPLEMENTED — Pending Validation
- Added an optional Socket.IO Streamlabs provider, donation models and media repository, tiered donation alert selection, alert history, and donation TTS.
- Missing Streamlabs URL or token disables the provider without blocking the rest of the application.
- The feature is present, but Story 1.2.1 did not independently validate the optional live Streamlabs integration. Future donation-provider improvements remain separate pending work.
- **Evidence:** [PR #93](https://github.com/gmblogref/TwitchChatBot/pull/93), `StreamlabsDonationSocketService`, `DonationAlertService`, and `DonationMediaRepository`.

### Raid Profile Image Display — PR #94

- **Status:** COMPLETE
- Raid handling resolves the raider's profile image through Helix, includes it in alert extra data, and renders the image in the browser alert layout.
- Profile-image lookup failures do not prevent the raid alert from continuing.
- **Evidence:** [PR #94](https://github.com/gmblogref/TwitchChatBot/pull/94), `HelixLookupService.GetProfileImageUrlByLoginAsync`, `TwitchAlertTypesService.HandleRaidAsync`, and `wwwroot/alerts.html`.

### Channel Point Mutual Exclusion — PR #95

- **Status:** COMPLETE
- Added redemption models, per-viewer reward-group locking, redemption orchestration, chat feedback, and Twitch cancel/refund attempts for blocked eligible redemptions.
- A viewer's lock does not affect other viewers, and cancellation failures are logged without crashing the bot.
- EventSub routes redemption messages through the orchestration service.
- **Evidence:** [PR #95](https://github.com/gmblogref/TwitchChatBot/pull/95), `ChannelPointRedemptionLockService`, `ChannelPointRedemptionService`, `ChannelPointRedemptionStatusService`, and their unit tests.

### JJustice Redemption Guard Change — PR #98

- **Status:** COMPLETE
- Removed the configured JJustice reward guard so the Justice rewards are no longer assigned to a mutual-exclusion lock group in current code.
- The reusable mutual-exclusion services, orchestration, feedback, and refund/cancel infrastructure introduced by PR #95 remain present; PR #98 did not remove the entire system.
- **Evidence:** [PR #98](https://github.com/gmblogref/TwitchChatBot/pull/98), the current empty lock-group configuration in `ChannelPointRedemptionLockService`, and updated lock-service tests.

### Gift Sub Bundle Suppression — PR #97

- **Status:** IMPLEMENTED — Pending Validation
- Bulk/community gift subscriptions track an expected gift count and suppress related individual gift events from the same gifter within a bounded time window.
- Normal single gifted subscriptions remain unsuppressed, and Hype Train alerts remain separate.
- The PR reported automated-test success but explicitly left live Twitch validation pending; no newer live-validation evidence was located.
- **Evidence:** [PR #97](https://github.com/gmblogref/TwitchChatBot/pull/97), `GiftSubBundleSuppressionService`, and `TwitchClientWrapperTests`.

## Repository Hardening and Documentation Stories

### Story 1.0 — Public Repository Audit

- **Status:** IMPLEMENTED — Pending Validation
- Repository artifacts show public-repository hygiene through ignored private configuration, a placeholder configuration template, documented setup, and protected pull-request integration rules.
- No dedicated Story 1.0 completion report or uniquely attributable PR was located during Story 1.3 review, so no historical runtime or test result is claimed here.
- **Evidence:** `.gitignore`, `README.md`, `TwitchChatBot/appsettings.example.json`, and the later configuration-hardening work in PR #99.

### Story 1.2 — Documentation and Dependency Inventory

- **Status:** IMPLEMENTED — Pending Validation
- The current repository documents its architecture, framework, configuration model, and release process, and all six project files provide the authoritative dependency inventory.
- No dedicated Story 1.2 completion report or uniquely attributable PR was located during Story 1.3 review; no dependency upgrade is attributed to this documentation/inventory entry.
- **Evidence:** `README.md`, `TwitchChatBot.sln`, and all six `.csproj` files.

### Configuration Security Hardening — PR #99

- **Status:** COMPLETE
- Added and validated the placeholder-only `appsettings.example.json` workflow, required configuration validation, user-facing startup errors for missing or malformed configuration, and optional handling for Streamlabs and OpenAI credentials.
- Local build output continues to copy private configuration for existing developer execution, while publish output excludes private configuration and includes the example template.
- Logging was adjusted to avoid emitting sensitive provider exception content, and README guidance warns against distributing build output containing local configuration.
- **Evidence:** [PR #99](https://github.com/gmblogref/TwitchChatBot/pull/99), `Program.cs`, `AppSettings.cs`, `AppSettingsTests.cs`, `README.md`, and `TwitchChatBot.UI.csproj`.

## Story 1.2.1 — .NET Framework & NuGet Dependency Modernization

- **Status:** COMPLETE
- **Pull request:** [PR #100](https://github.com/gmblogref/TwitchChatBot/pull/100), merged.
- **GitHub Actions:** both required PR checks, Build and Version bump check, completed successfully.

### Objective

Modernize the solution from .NET 8 to .NET 10 LTS, align approved dependencies with the new framework baseline, and update continuous-integration SDK selection without changing application behavior or architecture.

### Framework and SDK

- Previous framework baseline: .NET 8, with the Windows Forms UI targeting `net8.0-windows`.
- Final framework baseline:
  - `TwitchChatBot.UI`: `net10.0-windows`
  - `TwitchChatBot.Core`: `net10.0`
  - `TwitchChatBot.Data`: `net10.0`
  - `TwitchChatBot.Models`: `net10.0`
  - `TwitchChatBot.Testing`: `net10.0`
  - `TwitchChatBot.Tests`: `net10.0`
- Application version: 3.0.0.
- Supported IDE baseline: Visual Studio 2026 Community.
- The repository-root `global.json` selects .NET SDK 10.0.402, uses `latestPatch` roll-forward, and disallows prerelease SDKs.
- Windows 11 is the recommended development environment.

### Approved Dependency Changes

- Aligned direct `Microsoft.Extensions.*` references from 10.0.7 to 10.0.13 where direct references remain necessary.
- Replaced Core's legacy `Microsoft.AspNetCore.Http.Abstractions` package with the `Microsoft.AspNetCore.App` framework reference.
- Removed 14 redundant direct `Microsoft.Extensions.*` package references from the UI and Core projects after the .NET 10 NU1510 investigation; the UI's existing `Microsoft.AspNetCore.App` framework reference was preserved.
- Updated xUnit from 2.5.3 to 2.9.3.
- Updated Microsoft.NET.Test.Sdk from 18.5.1 to 18.11.0.
- The xUnit update removed the vulnerable `NETStandard.Library` 1.6.1 dependency path to `System.Net.Http` 4.3.0 and `System.Text.RegularExpressions` 4.3.0.
- No broad dependency modernization was performed. The existing versions of TwitchLib.Client, AWSSDK.Polly, SocketIOClient, Moq, FluentAssertions, coverlet.collector, and xunit.runner.visualstudio were preserved.

### Windows Forms Compatibility

- Corrected two .NET 10 WFO1000 compilation errors in `WheelSpinnerControl.cs` on `Items` and `RotationAngle`.
- Marked the runtime-populated `Items` property with `DesignerSerializationVisibility.Hidden`.
- Declared the `RotationAngle` default with `DefaultValue(0f)`.
- The attributes affect design-time serialization only; wheel rendering, rotation, item loading, and spin animation behavior remain unchanged.
- The Visual Studio 2026 WinForms designer was manually opened successfully after the correction.
- No designer-generated source changes were necessary.

### GitHub Actions and Version

- Updated the build workflow to use `actions/checkout@v7` and `actions/setup-dotnet@v6`.
- The build workflow selects the SDK through repository `global.json` with GA SDK quality.
- Updated the version-check workflow to use `actions/checkout@v7` without changing its then-current validation behavior.
- The existing build and version-check workflow behavior was otherwise preserved in Story 1.2.1.
- PR #100 subsequently passed both mandatory GitHub Actions checks.

### Developer Validation Results

- .NET SDK 10.0.402 was verified.
- NuGet restore passed with zero warnings after the NU1510 correction.
- The NuGet vulnerability audit reported no known vulnerable packages across all six projects.
- The deprecated-package audit identified only the expected legacy xUnit v2 packages.
- Debug solution build passed across all six projects.
- Release solution build passed across all six projects.
- Release clean rebuild succeeded.
- Automated tests passed: 164 passed, 0 failed, 0 skipped.
- Visual Studio 2026 Community successfully opened the solution and the main WinForms designer.
- Debug application startup succeeded.
- The Release application launched and operated successfully.
- Test Bot startup succeeded.
- Incoming Twitch chat was verified.
- Outgoing Twitch chat was verified through the First Chatter response.
- Browser alert display was verified.
- OBS alert display was verified after refreshing its browser source.
- The follow test alert and AWS Polly text-to-speech were verified.

### Known Limitations and Deferred Work

- Eight CS8602 nullable warnings remain in `TwitchChatBot.cs`; they were exposed by updated WinForms nullable annotations.
- xUnit v2 remains marked as legacy and was intentionally retained; migration to xUnit v3 is deferred.
- FluentAssertions 8.9.0 emitted a licensing notice; commercial-use licensing requires separate evaluation.
- Broader modernization of AWSSDK.Polly, TwitchLib, SocketIOClient, and the remaining test packages is deferred.
- Not every live EventSub event type was exercised.
- The optional Streamlabs integration was not independently verified.
- AWS Polly charges may apply to future live text-to-speech tests.

### Acceptance Status

**IMPLEMENTATION COMPLETE — DEVELOPER VALIDATED**

**PR #100 MERGED — REQUIRED GITHUB ACTIONS PASSED**

## Story 1.3 — AGENTS.md and FEATURE_HISTORY.md Refinement; Version-Check Correction

- **Status:** IN PROGRESS
- **Working branch:** `task/markdown-refinement` (developer managed).
- Refines repository guidance and reconstructs feature history from current implementation, merged pull requests, and confirmed completion reports.
- Corrects the version-check workflow to require a numerically greater `major.minor.patch` version for `.cs` or `.csproj` pull requests while preserving the documentation-only skip behavior.
- No application code, project file, dependency, application version, build workflow, or repository setting is changed by this story.
- Developer-owned pull-request checks and acceptance remain pending.
