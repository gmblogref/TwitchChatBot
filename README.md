# Twitch Chat Bot

Twitch Chat Bot is a Windows desktop application for running Twitch chat automation, stream alerts, text-to-speech, browser overlays, and related interaction tools from a Windows Forms control panel. The application runs locally and connects to configured external services; it is not a hosted web application.

- **Current application version:** 3.0.0
- **Framework:** .NET 10 LTS
- **Platform:** Windows only (`net10.0-windows`)
- **Supported development baseline:** Windows 11 and Visual Studio 2026 Community

## Current Capabilities

- Twitch chat connectivity, incoming-message handling, outgoing messages, reconnect behavior, and viewer updates.
- Twitch EventSub WebSocket subscriptions and routing for supported alerts and channel-point events.
- Queued alert and media playback through a local embedded ASP.NET Core host and WebSocket server.
- Browser and OBS-compatible alert and fullscreen overlays.
- Alerts for follows, subscriptions, resubscriptions, gifted subscriptions, cheers, raids, watch streaks, and channel-point redemptions.
- AWS Polly text-to-speech with configurable voices and neural-to-standard fallback.
- Configurable chat commands, First Chatter behavior, watch-streak tracking, and excluded-user handling.
- Wheel Spinner management, weighted selection, animations, and alert or command triggering.
- Raid profile-image lookup and display.
- Channel-point mutual-exclusion infrastructure and cancellation/refund attempts for blocked eligible redemptions.
- Optional Streamlabs donation events, tiered donation alerts, history, and text-to-speech.
- Optional AI-generated alert text for supported alert types.
- Alert history and missed-alert replay.
- Moderation and Nuke command support.
- Twitch moderator/VIP lookup and viewer-list functionality.
- Gift-sub bundle suppression that prevents related individual gift alerts from duplicating a community-gift alert.

The existing EventSub and Streamlabs functionality is accepted, but the .NET 10 modernization validation did not exercise every supported live EventSub type or independently revalidate the optional live Streamlabs provider. Gift-sub bundle suppression is implemented but still awaits live Twitch validation. See [FEATURE_HISTORY.md](FEATURE_HISTORY.md) for implementation history and validation boundaries.

## Windows Development Prerequisites

Install the following before opening the repository:

- Windows 11 recommended. The WinForms application is Windows-only.
- [Visual Studio 2026 Community](https://visualstudio.microsoft.com/) with the **.NET desktop development** workload.
- Git for Windows.
- The .NET 10 LTS SDK selected by [`global.json`](global.json): SDK `10.0.402`, `latestPatch` roll-forward, and prerelease SDKs disabled.

Confirm the selected SDK from the repository root:

```powershell
dotnet --version
```

The result should resolve through `global.json` to SDK `10.0.402` or an allowed later patch in the same feature band.

Framework-dependent execution outside a development SDK installation requires the matching .NET 10 Desktop Runtime and ASP.NET Core Runtime for the machine architecture.

## Clone and Open the Solution

Clone the repository using its configured public remote:

```powershell
git clone https://github.com/gmblogref/TwitchChatBot.git
Set-Location TwitchChatBot
```

Open [`TwitchChatBot.sln`](TwitchChatBot.sln) in Visual Studio 2026 Community. If Visual Studio does not select an executable project automatically, set `TwitchChatBot.UI` as the startup project.

## Local Configuration

The tracked [`TwitchChatBot/appsettings.example.json`](TwitchChatBot/appsettings.example.json) file defines the configuration shape and contains placeholders only. Create the required ignored base configuration from the repository root:

```powershell
Copy-Item .\TwitchChatBot\appsettings.example.json .\TwitchChatBot\appsettings.json
```

Replace placeholders with local values and valid local paths. The application loads configuration in this order:

1. `TwitchChatBot/appsettings.json` — required base settings.
2. `TwitchChatBot/appsettings.Development.json` — optional local overrides, recommended for credentials.

At a high level, configuration covers Twitch identities and authentication, EventSub endpoints, local media-map and media-folder paths, the embedded web host, AWS Polly voices and region, alert behavior, and optional Streamlabs and OpenAI settings. Detailed provider, OAuth, media, and environment setup is intentionally outside this onboarding guide.

See [docs/CONFIGURATION.md](docs/CONFIGURATION.md) for the complete key inventory, precedence rules, validation behavior, persisted runtime data, safe examples, and troubleshooting guidance.

See [docs/TWITCH_SETUP.md](docs/TWITCH_SETUP.md) for Twitch account ownership, developer-application registration, OAuth token and scope setup, EventSub/Helix behavior, and first-run verification.

Both local configuration files are ignored by Git. At build time they are copied beside the executable because configuration is loaded from the application directory. Placeholder values satisfy only part of the configuration shape and do not authenticate with external services.

## AI-Assisted Setup

Developers may use ChatGPT, Claude, Codex, or another AI coding assistant to help configure their own TwitchChatBot installation. Copy the prompt below into the assistant, keep all credential values private, and follow the setup one step at a time.

```text
"I'm setting up TwitchChatBot for my own Twitch channel.

First, read:
- AGENTS.md
- docs/CONFIGURATION.md
- docs/TWITCH_SETUP.md

Use these files as the authoritative source for how
this application is currently implemented.

Walk me through the setup one step at a time.

Help me:
1. Configure my Twitch Developer application.
2. Set up my broadcaster and bot accounts.
3. Generate OAuth tokens using the documented
   official Twitch tools.
4. Determine the required OAuth scopes.
5. Find my broadcaster and bot Twitch User IDs.
6. Configure my local appsettings.json.
7. Verify IRC, EventSub, and Helix connectivity.
8. Troubleshoot any authentication or connection errors.

Do not assume planned functionality is implemented.

Do not ask me to paste OAuth tokens, client secrets,
or other private credentials into this conversation.

Use placeholders for sensitive configuration values.

Do not modify application source code unless I
explicitly request a change.

Give me one setup step at a time and wait for me
to confirm completion before continuing."
```

## Build

Restore once from the repository root:

```powershell
dotnet restore TwitchChatBot.sln
```

Build the complete solution in Debug configuration:

```powershell
dotnet build TwitchChatBot.sln --configuration Debug --no-restore
```

Build the complete solution in Release configuration:

```powershell
dotnet build TwitchChatBot.sln --configuration Release --no-restore
```

Visual Studio's **Build Solution** command builds the selected solution configuration across all six projects.

## Run Locally

### Visual Studio

1. Open `TwitchChatBot.sln`.
2. Select `TwitchChatBot.UI` as the startup project.
3. Select the `Debug` configuration.
4. Confirm that local configuration is present and contains the settings required for the integrations you intend to use.
5. Press **F5** to run with the debugger, or **Ctrl+F5** to run without it.

Startup displays a configuration error and exits if the required `appsettings.json` file is missing, contains malformed JSON, or fails required-setting validation.

### Release executable

After a successful Release build, run:

```powershell
.\TwitchChatBot\bin\Release\net10.0-windows\TwitchChatBot.UI.exe
```

The framework-dependent executable is located at:

```text
TwitchChatBot/bin/Release/net10.0-windows/TwitchChatBot.UI.exe
```

The Release output must retain its supporting files and valid local configuration.

## External Integrations

### Twitch

Twitch chat uses `TwitchLib.Client`. EventSub WebSockets and Twitch Helix requests support alerts, subscriptions, channel-point handling, profile images, moderation, and role lookup. A developer must supply their own Twitch application details, account identifiers, and appropriate OAuth/access tokens.

Follow the [Twitch Developer and OAuth Setup Guide](docs/TWITCH_SETUP.md) to create the developer application, authorize the bot and broadcaster identities, configure the current scopes and token formats, and validate the integration safely.

### AWS Polly

AWS Polly provides text-to-speech. Configure a region and supported voices in local application settings, and supply AWS credentials through the normal AWS credential chain, such as environment variables or the shared AWS credentials file. Polly use may incur AWS charges.

Follow the [AWS Polly Text-to-Speech Setup Guide](docs/AWS_POLLY_SETUP.md) for credential resolution, least-privilege IAM permission, region and voice configuration, safe verification, failure behavior, and billing precautions.

### Streamlabs

Streamlabs donation handling is optional and uses a Socket.IO connection. Leaving its socket URL or token blank disables the provider without preventing the rest of the application from running. The accepted implementation was not independently revalidated against the live provider during the .NET 10 modernization.

Follow the [Streamlabs Donation Setup Guide](docs/STREAMLABS_SETUP.md) for the externally managed socket-token flow, donation media, connectivity checks, Polly dependency, and current limitations.

### OpenAI

AI-generated alert text is optional and uses the configured OpenAI Responses API endpoint, model, and API key. The visual alert continues when generation fails, but qualifying watch-streak, gift-bundle, and annual-resub paths currently have no speech fallback. Keep the API key only in ignored local configuration.

Follow the [OpenAI Alert-Text Setup Guide](docs/OPENAI_SETUP.md) for API-key and billing setup, the data sent for each supported alert type, event-specific speech fallbacks, costs, and troubleshooting.

## Browser and OBS Overlays

The UI starts an embedded ASP.NET Core host when the bot runtime is started. It serves browser-facing alert content and delivers queued alerts over a local WebSocket connection.

The configured web-host base URL exposes:

- `/alerts` for the alert overlay.
- `/fullscreen` for fullscreen browser content.

These pages can be opened in a local browser or configured as OBS Browser Sources. The actual host address and media location come from local configuration; the application is not deployed as a hosted website.

## Repository Structure

The solution contains six projects:

| Project | Responsibility |
| --- | --- |
| [`TwitchChatBot.UI`](TwitchChatBot/TwitchChatBot.UI.csproj) | Windows Forms UI, entry point, dependency-injection composition, EventSub and Streamlabs providers, embedded web host, and overlay assets. |
| [`TwitchChatBot.Core`](TwitchChatBot.Core/TwitchChatBot.Core.csproj) | Business logic, service contracts, Twitch chat, alerts, commands, TTS, moderation, replay, and runtime utilities. |
| [`TwitchChatBot.Data`](TwitchChatBot.Data/TwitchChatBot.Data.csproj) | JSON-backed repositories and persistence helpers. |
| [`TwitchChatBot.Models`](TwitchChatBot.Models/TwitchChatBot.Models.csproj) | Shared models and configuration accessors. |
| [`TwitchChatBot.Testing`](TwitchChatBot.Testing/TwitchChatBot.Testing.csproj) | Manual utilities used by the application's Testing tab; this is not the automated test project. |
| [`TwitchChatBot.Tests`](TwitchChatBot.Tests/TwitchChatBot.Tests.csproj) | xUnit unit tests for Core, Data, and Models behavior. |

Architecture and repository-working rules are maintained in [AGENTS.md](AGENTS.md).

## Automated Tests

Run the solution's automated unit tests from the repository root:

```powershell
dotnet test TwitchChatBot.sln --configuration Debug
```

The automated test project uses xUnit, Moq, and FluentAssertions. UI and live-provider behavior requires separate manual validation. The current GitHub Actions Build workflow restores and builds the solution but does **not** run the automated test suite.

## Contribution and Pull Requests

`main` is protected, and all integration into `main` goes through pull requests. Work on a task branch, keep changes limited to the approved story, and leave Git and pull-request operations to the developer responsible for the change.

Pull requests targeting `main` are governed by two required checks:

- **Build** — restores dependencies and builds the solution in Release configuration.
- **Version bump check** — enforces the application-version rule when `.cs` or `.csproj` files changed.

See [AGENTS.md](AGENTS.md) for repository conventions, test rules, architecture boundaries, and development responsibilities.

## Versioning

The application version is stored in `TwitchChatBot/TwitchChatBot.UI.csproj` and uses a numeric `major.minor.patch` format.

- A pull request containing `.cs` or `.csproj` changes must set a version numerically greater than the version on `main`.
- Routine application changes normally increment the patch component.
- A justified minor or major increase is allowed.
- Documentation-only and workflow-only pull requests may retain the current application version when no `.cs` or `.csproj` files changed.

Story 1.4 is documentation-only and retains version `3.0.0`.

## Security and Secrets

- Never commit OAuth tokens, access tokens, API keys, AWS credentials, client secrets, or personal configuration.
- Keep real values in ignored `appsettings.json` and `appsettings.Development.json` files.
- Do not replace the tracked example placeholders with real credentials.
- Local Debug and Release output can contain copied private configuration. Do not distribute `bin`, `obj`, or other generated output without inspecting and removing sensitive files.
- Published output excludes private application configuration and includes the example template, but previously generated folders must still be inspected before sharing.

No separate `SECURITY.md` policy currently exists. Report sensitive repository issues privately to the repository owner rather than placing credentials or exploit details in a public issue.

## License

TwitchChatBot is licensed under the [MIT License](LICENSE). Under its terms, developers may clone the source, modify it, build it, and run their own configured versions.

This repository provides source code for developer-led local builds; it does not provide a packaged application or installer. Dependencies and any third-party materials remain subject to their respective license terms.

## Additional Repository Documentation

- [AGENTS.md](AGENTS.md) — durable architecture, process, testing, security, and contribution rules.
- [FEATURE_HISTORY.md](FEATURE_HISTORY.md) — implemented capabilities, story outcomes, validation evidence, and known limitations.
- [docs/CONFIGURATION.md](docs/CONFIGURATION.md) — complete application configuration and runtime-settings reference.
- [docs/TWITCH_SETUP.md](docs/TWITCH_SETUP.md) — Twitch accounts, developer application, OAuth scopes and tokens, EventSub, Helix, and first-run verification.
- [docs/STREAMLABS_SETUP.md](docs/STREAMLABS_SETUP.md) — optional Streamlabs Socket.IO donation setup, media, validation, and limitations.
- [docs/OPENAI_SETUP.md](docs/OPENAI_SETUP.md) — optional OpenAI Responses API setup, supported alerts, fallbacks, security, and costs.
- [docs/AWS_POLLY_SETUP.md](docs/AWS_POLLY_SETUP.md) — AWS credentials, least-privilege Polly access, voices, speech output, and billing precautions.
