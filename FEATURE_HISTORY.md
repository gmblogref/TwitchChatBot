# Feature History

This file records implemented behavior and completed story outcomes. It is not the future-feature backlog. Statuses mean:

- **COMPLETE**: implementation is accepted or has explicit developer/merged-PR validation.
- **COMPLETE — Developer Confirmed**: the developer has explicitly confirmed story acceptance; any limitation in independently attributable historical evidence is documented separately.
- **IMPLEMENTED — Pending Live Validation**: implementation exists, but a required live validation gap remains.
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

- **Status:** COMPLETE
- `EventSubSocketService` maintains the EventSub WebSocket lifecycle, subscription setup, reconnect handling, and routing of supported events into alert and channel-point services.
- The existing functionality is developer accepted. Story 1.2.1 runtime validation did not exercise every supported live EventSub event type, so its modernization validation was not exhaustive.
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

- **Status:** COMPLETE
- `CommandAlertService` parses configured commands, resolves command media, supports special command behavior, and integrates with alert, moderation, role, TTS, and First Chatter services.
- Merged implementation history and developer acceptance confirm the existing functionality. No separate historical live-command acceptance report was located during Story 1.3 review.
- **Evidence:** merged command work including [PR #37](https://github.com/gmblogref/TwitchChatBot/pull/37), `TwitchChatBot.Core/Services/CommandAlertService.cs`, and `TwitchChatBot.Tests/Core/CommandAlertServiceTests.cs`.

### First Chatter Detection

- **Status:** COMPLETE
- First-chat participation is tracked per stream, excluded users are filtered, configured media can play, and fallback chat responses are supported.
- Story 1.2.1 developer validation confirmed the outgoing First Chatter response.
- Testing Tab isolation remains pending work and is not part of the accepted runtime behavior.
- **Evidence:** `TwitchChatBot.Core/Services/FirstChatterAlertService.cs`, `TwitchChatBot.Data/FirstChatterMediaRepository.cs`, and their unit tests.

### Watch Streak Tracking

- **Status:** COMPLETE
- Watch streak state tracks stream attendance, current and longest streaks, debounced persistence, and testing/replay safeguards.
- Merged implementation history and developer acceptance confirm the existing functionality. No separate historical live acceptance report was located during Story 1.3 review.
- **Evidence:** merged Watch Streak work including [PR #46](https://github.com/gmblogref/TwitchChatBot/pull/46), `TwitchChatBot.Core/Services/WatchStreakService.cs`, `TwitchChatBot.Data/WatchStreakRepository.cs`, and their unit tests.

### AI-Generated Alert Text

- **Status:** COMPLETE
- `AiTextService` generates bounded alert text for raids, watch streaks, resubscription milestones, and gift-sub bundles through the configured OpenAI client; unavailable or unsuccessful generation returns no generated line rather than blocking the alert path.
- **Evidence:** [PR #63](https://github.com/gmblogref/TwitchChatBot/pull/63), `TwitchChatBot.Core/Services/AiTextService.cs`, and `TwitchChatBot.Tests/Core/AiTextServiceTests.cs`.

### Alert History and Missed-Alert Replay

- **Status:** COMPLETE
- Alert history retains recent alert entries for the running application, and replay dispatches supported entries back through their corresponding alert, command, TTS, or Twitch handling path while suppressing duplicate history entries during replay.
- **Evidence:** [PR #32](https://github.com/gmblogref/TwitchChatBot/pull/32), `TwitchChatBot.Core/Services/AlertHistoryService.cs`, and `TwitchChatBot.Core/Services/AlertReplayService.cs`.

### Moderation and Nuke Commands

- **Status:** COMPLETE
- Moderation support resolves Twitch users and performs configured timeout actions through Helix. The Nuke command limits use per user until its reset interval and integrates with command handling.
- **Evidence:** merged command and moderation work including [PR #37](https://github.com/gmblogref/TwitchChatBot/pull/37), `TwitchChatBot.Core/Services/ModerationService.cs`, `TwitchChatBot.Core/Services/NukeService.cs`, and `TwitchChatBot.Core/Services/CommandAlertService.cs`.

### Twitch Role and Viewer-List Functionality

- **Status:** COMPLETE
- Viewer-list behavior is wired into the application, and `TwitchRoleService` resolves moderator and VIP logins through Helix for role-aware behavior.
- **Evidence:** [PR #16](https://github.com/gmblogref/TwitchChatBot/pull/16), [PR #87](https://github.com/gmblogref/TwitchChatBot/pull/87), `TwitchChatBot.Core/Services/TwitchRoleService.cs`, and `TwitchChatBot.Tests/Core/TwitchRoleServiceTests.cs`.

## Implemented Feature Milestones

### Wheel Spinner System — PRs #88 and #89

- **Status:** COMPLETE
- Added JSON-backed wheel storage, weighted selection, item ordering and visibility, shuffle behavior, wheel management, winner animation, and alert/command triggering from wheel results.
- Random selection is abstracted through `IRandomProvider` for targeted unit testing.
- The WinForms wheel tab and `WheelSpinnerControl` provide the runtime UI.
- **Evidence:** [PR #88](https://github.com/gmblogref/TwitchChatBot/pull/88), [PR #89](https://github.com/gmblogref/TwitchChatBot/pull/89), `WheelService`, `WheelRepository`, `WheelSpinnerControl`, and their tests.

### Streamlabs Donation Integration — PR #93

- **Status:** COMPLETE
- Added an optional Socket.IO Streamlabs provider, donation models and media repository, tiered donation alert selection, alert history, and donation TTS.
- Missing Streamlabs URL or token disables the provider without blocking the rest of the application.
- The existing functionality is accepted, but Story 1.2.1 did not independently revalidate the optional live Streamlabs provider. Future donation-provider improvements remain separate pending work.
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

- **Status:** IMPLEMENTED — Pending Live Validation
- Bulk/community gift subscriptions track an expected gift count and suppress related individual gift events from the same gifter within a bounded time window.
- Normal single gifted subscriptions remain unsuppressed, and Hype Train alerts remain separate.
- The PR reported automated-test success but explicitly left live Twitch validation pending; no newer live-validation evidence was located.
- **Evidence:** [PR #97](https://github.com/gmblogref/TwitchChatBot/pull/97), `GiftSubBundleSuppressionService`, and `TwitchClientWrapperTests`.

## Repository Hardening and Documentation Stories

### Story 1.0 — Public Repository Audit

- **Status:** COMPLETE — Developer Confirmed
- Repository artifacts show public-repository hygiene through ignored private configuration, a placeholder configuration template, documented setup, and protected pull-request integration rules.
- The developer confirmed completion. No dedicated Story 1.0 completion report or uniquely attributable PR was located during Story 1.3 review, so that historical attribution limitation is preserved and no historical runtime or test result is claimed here.
- **Evidence:** `.gitignore`, `README.md`, `TwitchChatBot/appsettings.example.json`, and the later configuration-hardening work in PR #99.

### Story 1.2 — Documentation and Dependency Inventory

- **Status:** COMPLETE — Developer Confirmed
- The current repository documents its architecture, framework, configuration model, and release process, and all six project files provide the authoritative dependency inventory.
- The developer confirmed completion. No dedicated Story 1.2 completion report or uniquely attributable PR was located during Story 1.3 review; that historical attribution limitation is preserved, and no dependency upgrade is attributed to this documentation/inventory entry.
- **Evidence:** `README.md`, `TwitchChatBot.sln`, and all six `.csproj` files.

### Earlier Configuration and Security Improvements — PRs #80, #81, and #83–#85

- **Status:** COMPLETE
- Earlier merged work documented local configuration setup, refined ignore rules for private application settings, removed an empty tracked configuration file, and documented the placeholder example-file workflow before the broader PR #99 hardening.
- **Evidence:** [PR #80](https://github.com/gmblogref/TwitchChatBot/pull/80), [PR #81](https://github.com/gmblogref/TwitchChatBot/pull/81), [PR #83](https://github.com/gmblogref/TwitchChatBot/pull/83), [PR #84](https://github.com/gmblogref/TwitchChatBot/pull/84), [PR #85](https://github.com/gmblogref/TwitchChatBot/pull/85), `.gitignore`, `README.md`, and `TwitchChatBot/appsettings.example.json`.

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

- **Status:** COMPLETE
- **Pull request:** [PR #101](https://github.com/gmblogref/TwitchChatBot/pull/101), merged.
- **Implementation branch:** `task/markdown-refinement` (developer managed).
- Refines repository guidance and reconstructs feature history from current implementation, merged pull requests, and confirmed completion reports.
- Corrects the version-check workflow to require a numerically greater `major.minor.patch` version for `.cs` or `.csproj` pull requests while preserving the documentation-only skip behavior.
- No application code, project file, dependency, application version, build workflow, or repository setting is changed by this story.
- The completed changes are present on `main` in the repository history for PR #101.

## Story 1.4 — README and Windows Developer Onboarding

- **Status:** COMPLETE — Developer Accepted — PR #102 Merged.
- **Pull request:** [PR #102](https://github.com/gmblogref/TwitchChatBot/pull/102), merged.
- **Working branch:** `task/readme-onboarding` (developer managed).
- Replaced the brief README with a public-facing project introduction and Windows-only developer onboarding guide covering implemented capabilities, prerequisites, repository cloning, configuration setup, Debug and Release builds, Visual Studio startup, local execution, and the Release executable path.
- Documented the Twitch, AWS Polly, optional Streamlabs, optional OpenAI, and local browser/OBS overlay integrations without replacing the detailed configuration, OAuth, provider, media, or environment documentation reserved for later stories.
- Documented the six-project architecture, automated-test command, protected-`main` pull-request workflow, required GitHub Actions checks, numeric application-version policy, and secret-handling requirements.
- Added the standard MIT License with verified repository-owner attribution, documented public source-code reuse permissions, and linked the README to the root `LICENSE` file. No application packaging or installer work was performed.
- Preserved validation boundaries: not every supported live EventSub event type was exercised during modernization, the optional live Streamlabs provider was not independently revalidated, and Gift Sub Bundle Suppression remains pending live Twitch validation.
- Documentation validation confirmed Markdown structure, relative links, referenced files, commands, SDK selection, Visual Studio baseline, solution projects, Release executable path, workflow names, version-bump behavior, and absence of exposed private paths or credentials. `git diff --check` completed without errors.
- A solution-level Release build completed successfully with the eight known CS8602 warnings and zero errors; automated tests were not run for this documentation-only story.
- Application code, projects, dependencies, configuration templates, workflows, behavior, and `AGENTS.md` were unchanged. Application version remains 3.0.0.

## Story 1.5 — Configuration Reference

- **Status:** IN PROGRESS — Documentation implementation complete; awaiting developer review and acceptance.
- **Implementation branch:** `task/configuration-reference` (created from the accepted Story 1.4 baseline).
- Added `docs/CONFIGURATION.md` as the detailed source-backed reference for all tracked application settings, including exact key paths, safe examples, types, requirement and secret classifications, commit policy, consumers, validation, fallbacks, and failure consequences.
- Documented the exact two-file JSON load order and precedence, unsupported application environment-variable and command-line overrides, local build and publish copy behavior, startup validation, AWS SDK credential resolution, and the separation between startup settings and persisted runtime JSON.
- Documented repository-backed persistence behavior, First Chatter and Watch Streak migrations/normalization, and settings that are unused, legacy, accessor-only, or ambiguous rather than presenting them as active controls.
- Added safe representative JSON, troubleshooting, and security guidance without including private values, and linked the detailed reference from the README while preserving the Story 1.4 onboarding flow.
- Documentation validation compared the inventory against `appsettings.example.json`, `AppSettings` accessors and validation, production consumers, configuration tests, project copy rules, and JSON repositories. Markdown links, JSON examples, secret exposure, and whitespace were checked; no application tests were run for this documentation-only story.
- No application code, project files, configuration files, dependencies, workflows, behavior, or `AGENTS.md` were changed. Application version remains 3.0.0.

## Story 1.6 — Twitch Developer and OAuth Setup Guide

- **Status:** ACCEPTED — Documentation complete; PR/merge pending.
- **Developer acceptance:** Granted on 2026-10-20.
- **Source control:** Developer acceptance complete; commit, push, pull request, and merge to protected `main` remain pending.

### Documentation Deliverables

- Created `docs/TWITCH_SETUP.md` as the source-backed Twitch developer and OAuth onboarding guide.
- Updated `README.md` with Twitch setup navigation and ready-to-copy AI-Assisted Setup instructions.
- Documented broadcaster and bot accounts, OAuth tokens, required and feature-dependent scopes, token ownership, validation, and manual renewal.
- Documented current IRC/TwitchLib, EventSub WebSocket, and Twitch Helix responsibilities and authorization requirements.
- Documented exact local configuration keys and placement, configuration precedence, first-run verification, troubleshooting, credential security, and existing implementation limitations.

### Phase C Acceptance Audit

- The read-only acceptance audit passed with one documentation correction required.
- Incorrect moderator/VIP pagination statements were corrected to reflect that both Helix lookup methods follow Twitch pagination cursors.
- Subsequent documentation validation passed, including relative links, referenced files, Markdown structure, credential safety, pending source-control language, and application version verification.
- No acceptance blockers remain.

### Scope Confirmation

- Changes were documentation-only; no application source code was changed.
- No authentication, IRC, EventSub, Helix, configuration-schema, dependency, workflow, or application behavior changes were made.
- Application version remains 3.0.0.
- No builds or automated tests were required for this documentation-only story.

## Story 1.7 — Optional Integration Documentation

- **Status:** COMPLETE — Developer Confirmed — PR #105 Merged.
- **Pull request:** [PR #105](https://github.com/gmblogref/TwitchChatBot/pull/105), merged.
- Added `docs/STREAMLABS_SETUP.md` for the current Socket.IO donation provider, externally managed socket-token flow, exact settings, donation payload and media handling, Polly dependency, safe verification, failure isolation, and known limitations.
- Added `docs/OPENAI_SETUP.md` for the direct Responses API integration, API-key and billing setup, supported alert prompts, data sent, model and threshold settings, security, costs, troubleshooting, and event-specific speech fallback behavior.
- Added `docs/AWS_POLLY_SETUP.md` for AWS SDK for .NET v4 credential resolution, least-privilege `polly:SynthesizeSpeech` permission, region and voice configuration, MP3 output, failure behavior, pricing, budgets, and billing precautions.
- Updated the README with concise navigation to all three provider guides while preserving the existing onboarding flow.
- Corrected the configuration reference to distinguish visual-alert continuation from missing speech fallbacks for qualifying watch streaks, gift bundles, and annual resubscription milestones, and documented the implemented `{streak}` watch-streak placeholder.
- Verified external guidance against official Streamlabs, OpenAI, and AWS documentation and the SocketIOClient project documentation, while distinguishing provider behavior from repository-confirmed application behavior.
- Changes are documentation-only. No application code, project files, configuration schema, dependencies, workflows, or application behavior were changed. Application version remains 3.0.0.
- Documentation validation completed without a Story-specific build or automated-test run. The developer confirmed completion, and the changes are present on `main` through PR #105.

## Story 1.8 — Media, Assets, and Environment Setup Documentation

- **Status:** DEVELOPER ACCEPTED — PR/MERGE PENDING.
- **Developer acceptance:** Granted on October 21, 2026.
- **Source control:** Developer acceptance complete; pull request and merge to protected `main` remain pending.
- Added `docs/MEDIA_ENVIRONMENT_SETUP.md` as the source-backed guide for Windows/.NET requirements, private media layout, path resolution, all seven JSON files, alert mappings, supported formats, codec boundaries, Wheel Spinner dispatch, browser/OBS behavior, queue/offline behavior, first-run setup, missing-file and failure behavior, Testing-tab production side effects, and troubleshooting.
- Added seven sanitized valid-JSON templates under `docs/examples/media` for Twitch alerts, excluded users, First Chatter, commands, watch-streak state, Wheel definitions, and donation media.
- Documented that the example filenames are configurable through `MediaMapFiles:*`, remain documentation-only, contain no real media, and are not automatically copied or seeded into runtime data.
- Corrected donation media examples in the Streamlabs documentation to use paths relative to `MediaBase:TwitchAlertsFolder` and linked the README and configuration reference to the new guide and templates.
- Reconciled Story 1.7 with the developer-confirmed PR #105 merge already present on `main`.
- Changes are documentation and sanitized example data only. No application code, project files, configuration schema, dependency, workflow, or runtime behavior changed. Application version remains 3.0.0.
- JSON syntax, Markdown links, model properties, media descriptions, alert categories, Wheel integration, secret safety, and changed-file scope were checked without running builds or automated tests. Developer acceptance is complete; pull request and merge remain pending.

## Story 1.9 — Architecture and Testing Documentation

- **Status:** IMPLEMENTED — Pending Developer Acceptance.
- Added `docs/ARCHITECTURE.md` as the source-verified guide to all six projects and their references, dependency compromises, WinForms/DI composition, configuration initialization, start/stop/shutdown order, background-provider readiness, TwitchLib/IRC and EventSub responsibilities, Helix, alert/WebSocket/overlay flow, Polly/OpenAI/Streamlabs integrations, JSON persistence, Wheel behavior, failure boundaries, and current limitations.
- Added `docs/TESTING.md` with exact test-package versions, repository test organization, static test inventory, verified coverage and gaps, restore/build/test/launch commands, mocks and isolation boundaries, automated-test side effects, manual acceptance procedures, Testing-tab production risks, CI behavior, and the protected-`main` developer workflow.
- Documented that `TestUtilityService.TriggerFirstChatTestAsync` supplies the configured `TWITCH_BOT_ID` as the simulated chatter user ID and that Testing-tab actions can send real Twitch messages, persist state, invoke providers, or incur external charges.
- Documented that EventSub and Streamlabs startup schedules background work and does not establish external readiness when `StartAsync` returns, and recorded the subscribed EventSub notification types that currently lack handlers.
- Recorded the Build workflow's plural `tasks/**` push trigger versus the repository's documented singular `task/...` implementation branches without changing the workflow. Confirmed that pull requests targeting `main` still trigger Build and that the version check skips its bump requirement when no `.cs` or `.csproj` files changed.
- Added `PROJECT_ID: TWITCH-CHATBOT` and Project Identity Guard v1.0 to `AGENTS.md`, requiring formal handoffs to match the repository identity before modifications.
- Updated README navigation to the new architecture and testing guides.
- Applied Phase C developer-review corrections: fixed the Release executable name, separated intended Stop Bot operations from source-confirmed lifecycle gaps and developer-observed Stop/Start unreliability, and updated manual acceptance guidance without claiming a lifecycle fix or established root cause.
- Documentation links, project references, package versions, commands, lifecycle descriptions, integration responsibilities, workflow behavior, private-path/credential safety, version, and changed-file scope were checked without running builds, automated tests, the application, or live integrations.
- Changes are documentation-only. No application code, automated tests, project files, dependencies, configuration schemas, workflows, or runtime behavior changed. Application version remains 3.0.0.

## Story 1.10 — Public Repository Hardening and Developer Onboarding Acceptance

- **Status:** DOCUMENTATION CORRECTIONS IMPLEMENTED — Awaiting Developer Validation and PR.
- Completed an independent clean-clone acceptance review and applied the approved final documentation and example-configuration corrections in the original development repository.
- Updated automated-test onboarding to recommend a filtered clean-setup command that excludes the four `TtsServiceTests` and two `WheelRepositoryTests`, documents their AWS/media/file side effects, and preserves the complete suite as a controlled-environment option.
- Added active Fluent Assertions licensing guidance, documented the Utilities and Logging tab locations and session-only Alert History replay behavior, added a no-synthesis Polly verification path, and corrected the example watch-streak placeholder from `{streakCount}` to `{streak}`.

### Acceptance Evidence

- Independent clean clone completed successfully.
- NuGet restore completed successfully.
- Debug and Release solution builds completed successfully.
- Twitch connection and incoming/outgoing chat were verified.
- EventSub subscription registration was verified.
- Browser and OBS overlays were verified with GIF, MP4, and MP3 playback.
- Alert playback cleanup was verified.
- Wheel loading, spinning, result popup, and persistence were verified.
- Alert History recording and replay were verified during the running session.
- Clean application shutdown was verified.
- The filtered automated-test selection completed with 158 passed, 0 failed, and 0 skipped.
- The acceptance-clone Git working tree was clean.
- PR #108 Build and Version bump checks passed.
- All required Story 1.10 documentation files were present.

### Acceptance Limitations

- Six automated tests were excluded: four `TtsServiceTests` and two `WheelRepositoryTests`.
- Live Streamlabs donation delivery was not tested.
- Live OpenAI generation and fallback behavior were not tested.
- Controlled missing-credential AWS Polly behavior was not tested.
- Complete AWS Polly synthesis acceptance was not performed.
- Wheel-triggered alert integration was not tested.
- Twitch event coverage was not exhaustive.
- Configuration-error user experience was not tested.
- No main-branch CI run was available; the confirmed checks were for PR #108.

### Known Follow-Up Findings

- Eight existing CS8602 warnings remain assigned to Story 1.11.
- A blank OpenAI API key can still result in failed outbound requests on qualifying paths.
- Some qualifying AI speech paths lack fallback text when generation fails.
- TTS automated-test isolation remains a concern because tests construct the real service and can discover AWS credentials.
- Missing mapped media can suppress an entire alert.
- Streamlabs amount parsing remains a potential follow-up concern.
- Logging tab usability improvements remain future work.

The final corrections are documentation and sanitized example-configuration changes only. They do not fix the follow-up findings or change production application behavior. No full unfiltered test pass is claimed for Story 1.10.

## Story 1.11 — Compiler Warning Cleanup

- **Status:** IMPLEMENTED — Pending Developer Validation.
- **Implementation branch:** `task/fixing-warnings` (developer managed).
- Corrected the eight previously reported CS8602 warnings in `ConfigureEntriesGrid()` caused by repeated nullable WinForms string-keyed column lookups.
- Cached the `Label`, `Weight`, `Hidden`, `Position`, and `Id` lookups in nullable `DataGridViewColumn?` local variables and guarded all property access through those cached references.
- Preserved the existing column formatting, sizing, visibility, and missing-column behavior. No intentional application behavior changes were made.
- Modified `TwitchChatBot/TwitchChatBot.cs`, `TwitchChatBot/TwitchChatBot.UI.csproj`, and `FEATURE_HISTORY.md` only.
- Increased the application version from 3.0.0 to 3.0.1 under the repository's source-change version policy.
- Developer Debug and Release builds and automated tests remain pending; the warning cleanup is not yet compiler-verified.

## Story 1.12 — UI Labels and Readability Cleanup

- **Status:** COMPLETE.
- Developer validation confirmed that the Release build and automated tests passed, and the Story 1.12 UI work was accepted. Application version 3.0.2 includes the completed Story 1.12 and Story 1.13 changes.

### Slice A — Labels and Readability Cleanup

- **Status:** COMPLETE.
- Simplified the visible Main Chat, Utilities, Testing, and Logging tab captions while leaving the Wheel caption unchanged.
- Clarified the Reset First Chatters, Text to Speech, Add Entry, Remove Entry, and Application Log labels.
- Made the stream-chat and viewer-list displays read-only so displayed runtime information cannot be edited manually.
- Adjusted only the affected button dimensions and positions needed to accommodate the longer captions without overlap.
- Modified `TwitchChatBot/TwitchChatBot.Designer.cs` and `FEATURE_HISTORY.md` only. No identifiers, event handlers, services, logging behavior, Testing-tab behavior, or application logic were changed.

### Slice B — UI Tooltips

- **Status:** COMPLETE.
- Added one shared WinForms `ToolTip` component to the main form and concise guidance for the Utilities, Wheel, Testing, and main bot-lifecycle controls whose effects are not fully apparent from their labels.
- Added header tooltips to the generated Weight and Hidden Wheel columns to explain weighted selection and hidden-entry behavior.
- Documented Testing-tab inputs and actions without describing them as isolated; relevant tooltips warn that production services can send chat, update data, trigger alerts or TTS, call external APIs, or incur usage.
- Modified `TwitchChatBot/TwitchChatBot.Designer.cs`, `TwitchChatBot/TwitchChatBot.cs`, and `FEATURE_HISTORY.md` only. No captions, layouts, event handlers, services, logging behavior, Testing-tab behavior, or other application logic were changed.
- Follow-up developer validation identified missing Wheel coverage; added tooltips for wheel selection, New Wheel, Add Entry, Remove Entry, the editable entry grid and Name column, and winner confirmation.
- Preserved the existing Delete Wheel, Shuffle, Save, Spin, Weight, and Hidden tooltip descriptions without changing Wheel behavior, persistence, captions, or layout.

### Slice C — Logging Improvements

- **Status:** COMPLETE.
- Added Clear Log to clear only the text currently displayed on the Logging tab; the logger provider, UI bridge, persistent data, and delivery of subsequent log messages are unchanged.
- Added Copy All to copy the displayed log text without modifying it, return safely for an empty log, and show a nonfatal warning if clipboard access fails.
- Added concise tooltips for both controls and positioned them in the existing space beneath the log display without changing other tabs.
- Modified `TwitchChatBot/TwitchChatBot.Designer.cs`, `TwitchChatBot/TwitchChatBot.cs`, and `FEATURE_HISTORY.md` only. Logging configuration, levels, categories, formatting, generation, and forwarding were not changed.

### Slice D1 — New Wheel Entry Grid Fix

- **Status:** COMPLETE.
- Corrected the New Wheel handler to locate the bound `Label` column, displayed as Name, instead of checking for the nonexistent `DisplayName` grid column.
- The first new entry's Name cell is now assigned as the current cell and placed into edit mode after the new wheel is loaded.
- Modified `TwitchChatBot/TwitchChatBot.cs` and `FEATURE_HISTORY.md` only. Wheel creation, selection, persistence, weighted spinning, labels, layout, and tooltips were unchanged.

### Slice D2 — Unsaved Changes on Window Close

- **Status:** COMPLETE.
- Corrected the form-closing path to cancel and return immediately when the user declines to discard unsaved Wheel changes, preserving the current entries and editing state without beginning shutdown.
- Added shutdown-in-progress and shutdown-complete guards so asynchronous shutdown runs only once and the final scheduled close does not repeat the confirmation or shutdown sequence.
- Preserved normal shutdown when no unsaved changes exist or the user confirms discarding them. Wheel save and persistence logic, confirmation wording, and Start Bot and Stop Bot behavior were unchanged.
- Modified `TwitchChatBot/TwitchChatBot.cs` and `FEATURE_HISTORY.md` only.

### Slice D3 — Start/Test Bot Button State Fix

- **Status:** COMPLETE.
- Updated the Start Bot, Test Bot, and Stop Bot enabled states from `ChatBotController.IsStarted` after each start or stop attempt, including startup failures handled internally by the controller.
- Start Bot and Test Bot now remain enabled and Stop Bot remains disabled when startup fails; successful startup disables both start options and enables Stop Bot. Stop Bot is also disabled in the form's initial stopped state.
- Reset testing mode when Test Bot startup does not reach the started state, preventing a failed test startup from leaving `IAppFlags.IsTesting` enabled.
- Modified `TwitchChatBot/TwitchChatBot.cs`, `TwitchChatBot/TwitchChatBot.Designer.cs`, and `FEATURE_HISTORY.md` only. Startup and shutdown orchestration and all integration behavior were unchanged.

### Slice D4A — End-to-End Skip and Reset TTS

- **Status:** COMPLETE.
- Tagged generated TTS alerts with sequence metadata so speech can be canceled independently across synthesis, the shared alert queue, WebSocket delivery, and browser or OBS playback.
- Skip TTS now stops the TTS item actively playing on connected overlays; when no overlay is playing speech, it cancels only the synthesis item that was current when Skip was requested. Later queued speech remains available, and a no-speech request is a safe no-op.
- Reset TTS now cancels current synthesis, clears queued synthesis jobs, removes matching pending TTS alerts, stops matching active or browser-pending TTS through a sequence cutoff, and preserves speech submitted after the reset request.
- Added targeted `skipTts` and `resetTts` control messages to the existing WebSocket transport. Each alert overlay tracks its active TTS media element, stops and unloads only matching TTS audio, and reports active TTS alert IDs to the server.
- Preserved non-TTS alerts and their queue order. Ordinary MP3, video, GIF, fullscreen, and text alerts are not tagged as TTS and are not stopped or removed by these controls.
- Added synchronization around TTS queue/current-item cancellation, guarded alert/cancellation broadcast ordering, and best-effort deletion of partial or removed temporary TTS files.
- Updated the Skip and Reset tooltips to describe generation, playback, and pending-speech behavior accurately.
- Modified `TwitchChatBot.Models/AlertItem.cs`, `TwitchChatBot.Core/Services/Contracts/IAlertService.cs`, `TwitchChatBot.Core/Services/Contracts/IWebSocketServer.cs`, `TwitchChatBot.Core/Services/TtsService.cs`, `TwitchChatBot.Core/Services/AlertService.cs`, `TwitchChatBot.Core/Services/WebSocketServer.cs`, `TwitchChatBot/TwitchChatBot.Designer.cs`, `TwitchChatBot/wwwroot/alerts.html`, and `FEATURE_HISTORY.md` only.

### Slice D4B — Wheel Save Atomicity

- **Status:** COMPLETE.
- Wheel entry saves now validate the complete staged grid before persistence, rejecting empty names, nonpositive weights, and duplicate trimmed names without deleting or updating existing stored entries.
- Replaced the previous per-entry remove, add, and update sequence with one validated whole-item-list save. Existing alert mappings are preserved for retained entries, and saved positions follow the displayed grid order.
- `SaveEntriesAsync()` and the new `IWheelService.SaveItemsAsync()` return explicit success or failure results. A rejected or failed save leaves the grid, unsaved-change state, and Save button available for correction; the Save-before-Shuffle path no longer continues when saving fails.
- The service builds a detached wheel snapshot and the repository publishes that snapshot in memory only after its existing temporary-file write and file replacement succeeds. This provides one atomic file-replacement boundary, but it is not a database transaction or a multi-process concurrency guarantee.
- Modified `TwitchChatBot/TwitchChatBot.cs`, `TwitchChatBot.Core/Services/Contracts/IWheelService.cs`, `TwitchChatBot.Core/Services/WheelService.cs`, `TwitchChatBot.Data/WheelRepository.cs`, and `FEATURE_HISTORY.md` only. Weighted selection, Spin, Shuffle implementation, Wheel layout, captions, and tooltips were unchanged.

### Slice D4C — Wheel Shuffle and Spin State Handling

- **Status:** COMPLETE.
- Added one shared Save, Discard, or Cancel decision flow for Spin and Shuffle when the entry grid contains unsaved changes. Save proceeds only after D4B reports success, Discard reloads persisted entries before continuing, and Cancel preserves the staged grid without running the requested action.
- Failed validation or persistence prevents Spin and Shuffle and leaves staged edits available for correction. The two handlers do not issue duplicate unsaved-change prompts.
- Spin now reacquires the persisted wheel after resolving unsaved changes, ensuring weighted selection and animation use the same saved or reloaded entry set rather than a stale combo-box snapshot.
- Spin is enabled only when a wheel is selected, the displayed grid contains at least one non-hidden entry, and a spin is not already running. Its state is refreshed during initialization, wheel and entry loading, wheel selection, grid edits, entry additions and removals, saving, discarding, shuffling, and spin completion; unsaved changes alone do not disable it.
- Empty-wheel loading now clears the spinner control and resets the save state, preventing entries from a previously selected wheel from remaining displayed.
- Modified `TwitchChatBot/TwitchChatBot.cs` and `FEATURE_HISTORY.md` only. Weighted selection, spin animation, winner confirmation, persisted Shuffle implementation, Wheel persistence architecture, layouts, captions, and tooltips were unchanged.

### Slice D4D — Testing Tab Async Fixes

- **Status:** COMPLETE.
- Changed the Testing-service Follow contract from `void` to `Task` and now await the production `HandleFollowAsync()` operation through `TestUtilityService` and the WinForms Follow handler.
- Updated the Watch Streak WinForms handler to await the existing asynchronous Testing-service operation through completion.
- Consolidated all 15 Testing-tab actions—Follow, Subscription, ReSub, Raid, Gift Subs, Mystery Gifts, Hype Train, Hype Train End, Watch Streak, Donations, Cheer, Channel Points, Commands, First Chat, and Text to Speech—through one shared asynchronous runner.
- Every submitted Testing action now observes propagated failures, logs the exception, displays a nonfatal test-failure warning, and restores its button state through nested `finally` cleanup. Existing blank or invalid-input no-op behavior remains unchanged and does not submit work.
- The shared runner tracks active operations by button, preventing overlapping submissions for the same action while allowing unrelated Testing buttons to remain independent.
- Developer validation found that queue-only production tasks can complete before WinForms repaints a disabled button. Each Testing action now measures from its accepted click and retains its independent guard and disabled state until both the operation has completed or failed and the Windows-configured `SystemInformation.DoubleClickTime` interval has elapsed.
- The repeat-click wait uses `Task.Delay` only for the remaining portion of the system interval, does not block the UI thread, and does not wait for alert dispatch, browser acknowledgement, playback, or `DONE`.
- Modified `TwitchChatBot.Testing/ITestUtilityService.cs`, `TwitchChatBot.Testing/TestUtilityService.cs`, `TwitchChatBot/TwitchChatBot.cs`, and `FEATURE_HISTORY.md` across Slice D4D. Production alert, command, donation, First Chatter, and TTS behavior; test inputs and payloads; AI/TTS fallback behavior; layouts; captions; and tooltips were unchanged.

### Slice D4E — AI TTS Fallback Fixes

- **Status:** COMPLETE.
- ReSub milestone, Mystery Gift, and Watch Streak alerts now retain their existing non-AI user-message or configured-template path whenever eligible AI generation does not return usable text, while successful AI text continues to be spoken exactly once.
- Null, empty, and whitespace AI responses now produce a warning and use the fallback. Unexpected AI-generation exceptions are logged as warnings and also use the fallback without stopping alert processing.
- ReSub and Watch Streak use their sanitized user message when present, otherwise their existing configured template and hard-coded template default. Mystery Gifts use the existing configured template and hard-coded template default.
- Existing ReSub milestone, Mystery Gift count, and Watch Streak threshold eligibility rules are unchanged. Alert enqueueing, gift-sub bundle suppression, voice selection, TTS service behavior, and graceful degradation when speech synthesis is unavailable are unchanged.
- Modified `TwitchChatBot.Core/Services/TwitchAlertTypesService.cs` and `FEATURE_HISTORY.md` only. Follow and Testing protections, TTS Skip/Reset, Wheel, Logging, and unrelated alert behavior were unchanged.

### Slice D4F — Final UI Hardening

- **Status:** COMPLETE.
- Added one shared Start Bot/Test Bot startup-in-progress guard. An accepted startup immediately disables both startup buttons, rejects rapid repeat and cross-button startup attempts, and restores all three bot button states from `ChatBotController.IsStarted` when startup completes or fails.
- Test Bot mode is enabled only for its accepted startup path and is reset when that startup does not reach the started state. Startup service orchestration and integration behavior were unchanged.
- Reviewed the D4C Spin enabled-state flow across wheel selection, empty wheels, entry addition/removal, Hidden edits, save/discard reloads, and spin start/completion. Existing refresh coverage handles those paths. Corrected the confirmed decline-selection mismatch by restoring the previously selected wheel and refreshing Spin when the user refuses to discard staged edits.
- Added consistent, nonmodal Testing-tab validation feedback through the existing shared tooltip. Invalid input is focused and retained for correction instead of silently doing nothing or substituting a default value.
- Cheer bits, raid viewers, ReSub months, Mystery Gift count, and Watch Streak count now require positive whole numbers. Donation amount requires a positive decimal. Channel Points reward, Commands input, First Chat username, and TTS text now require nonblank text. Invalid submissions do not reach production services.
- Preserved valid Testing payloads, the D4D per-button repeat-click policy, all available tests, shared-field meanings, production alert eligibility, AI TTS fallback behavior, TTS Skip/Reset, Wheel persistence, and weighted selection.
- Remaining limitations: bot startup completion still reflects `ChatBotController.IsStarted`, not confirmed readiness of every external provider, and Testing actions remain connected to production services.
- Modified `TwitchChatBot/TwitchChatBot.cs` and `FEATURE_HISTORY.md` only.

### Phase C/D — User Guide Integration and PR Preparation

- **Status:** COMPLETE.
- Added matching Markdown and Word User Guides as operator-facing documentation for Getting Started, the persistent bot controls, all five application tabs, TTS controls, Alert History, Wheel management and editing, Testing-tab behavior, Logging controls, and troubleshooting.
- Integrated five supplied screenshots under `docs/images/` for Main Chat, Utilities, Wheel, Testing, and Logging, and embedded the same five images directly in the Word guide. The accepted Wheel screenshot retains its minor button-edge clipping from resizing.
- Documented the final Story 1.13 global Wheel center-branding behavior and linked to `docs/CONFIGURATION.md` for authoritative configuration details.
- Organized the README documentation map by installation, application use, configuration, optional integrations, and developer information; added cross-links among the guides; and corrected stale OpenAI fallback descriptions in existing setup documentation to match Slice D4E.
- This documentation phase did not change runtime behavior. Final review confirmed that the Markdown guide's five screenshot references are portable and resolve, the Word guide embeds all five images, and both formats describe version 3.0.2 consistently.

## Story 1.13 — Optional Wheel Center Brand Image

- **Status:** COMPLETE.
- Renamed and repurposed the unfinished `UiMedia:SpinButton` setting as the active optional `UiMedia:WheelCenterBranding` setting. It supplies one global center-branding image shared by every wheel; the actual text SPIN button and its behavior remain unchanged.
- Relative image paths resolve under `MediaBase:TwitchAlertsFolder`, and absolute image paths are also supported. PNG, JPG, and JPEG files are accepted. An empty or missing setting displays no image, while a missing, unsupported, invalid, or corrupt file leaves the wheel usable with an empty center.
- The renderer keeps the branding image centered and stationary while the wheel spins, preserves aspect ratio and PNG transparency, applies circular clipping, and uses an image diameter equal to 20% of the wheel diameter. GIF support is deferred.
- Weighted selection, spin animation, and winner calculation remain unchanged. Story 1.12 wheel-entry atomic saves, detached snapshots, empty-wheel handling, and spin-state handling are also preserved.
- The earlier per-wheel image-management implementation was removed. Branding is not stored in individual wheel records, and the runtime `wheelsMediaMap.json` file and its existing wheel records were not modified.
- Changing the configured image path does not require recompilation; application restart is required to reload the image.
- Modified `TwitchChatBot.Models/AppSettings.cs`, `TwitchChatBot.Models/Wheel.cs`, `TwitchChatBot.Data/Contracts/IWheelRepository.cs`, `TwitchChatBot.Data/WheelRepository.cs`, `TwitchChatBot.Core/Services/Contracts/IWheelService.cs`, `TwitchChatBot.Core/Services/WheelService.cs`, `TwitchChatBot/Controls/WheelSpinnerControl.cs`, `TwitchChatBot/TwitchChatBot.cs`, `TwitchChatBot/TwitchChatBot.Designer.cs`, `TwitchChatBot/appsettings.example.json`, `TwitchChatBot.Tests/Models/AppSettingsTests.cs`, `TwitchChatBot.Tests/Core/WheelServiceTests.cs`, `TwitchChatBot.Tests/Data/WheelRepositoryTests.cs`, `docs/CONFIGURATION.md`, and `FEATURE_HISTORY.md`. The local ignored `TwitchChatBot/appsettings.Development.json` was also renamed to the new setting while preserving its configured path.
- Developer validation confirmed that the Release build and automated tests passed, the global wheel branding rendered correctly, and the 20% center-image size was accepted.
