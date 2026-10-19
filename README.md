# Twitch Chat Bot (.NET 8)

## Overview

Twitch Chat Bot is a .NET 8 WinForms desktop application that integrates with Twitch Chat and EventSub WebSocket APIs to provide real-time alert handling and interactive stream automation. It processes events such as cheers, subscriptions, watch streaks, commands, and channel point redemptions, triggering configurable media and logic defined within the application.

The system is built to be fully self-hosted, extensible, and version-controlled, allowing complete ownership of alert behavior and stream integrations.

---

## Features

- Real-time Twitch Chat integration  
- EventSub WebSocket support  
- Custom alert handling for:
  - Subscriptions (new, resub, gifted)
  - Cheers (bits)
  - Watch streak tracking
  - Channel point redemptions
  - Custom chat commands
- JSON-based media mapping and configuration  
- Self-hosted alert playback system  
- Version-controlled release process with semantic versioning  
- GitHub Actions CI enforcement:
  - Build validation on pull requests
  - Required version bump on code changes  

---

## Architecture

The application is structured using a layered design:

- **UI Layer (WinForms)** – Desktop interface and runtime control  
- **Core Layer** – Business logic and alert handling  
- **Data Layer** – Repository-based JSON configuration management  
- **Services** – Twitch, EventSub, media playback, and alert orchestration  

The system uses dependency injection and separation of concerns to keep alert logic modular and maintainable.

---

## Technology Stack

- .NET 8  
- WinForms  
- Twitch EventSub WebSockets  
- GitHub Actions (CI)  
- JSON-based configuration repositories  

---

## Configuration

The tracked `TwitchChatBot/appsettings.example.json` file defines the configuration structure and contains placeholders only. Copy it to `TwitchChatBot/appsettings.json` for required base settings. Store local credentials in `TwitchChatBot/appsettings.Development.json`; both local files are ignored by Git and must never be committed.

Configuration loads in this order:

1. `appsettings.json` supplies required base settings.
2. `appsettings.Development.json`, when present, overrides matching base values and is the recommended location for local credentials.

The Streamlabs donation integration and OpenAI API key are optional. Leaving the Streamlabs URL or token blank disables that integration. All other runtime-used keys represented by the example template are validated at startup; placeholder credentials satisfy the configuration schema but do not authenticate against external services.

Local builds continue to copy `appsettings.json` and `appsettings.Development.json` beside the executable so the existing development workflow works. Publish output excludes those local files and includes only `appsettings.example.json`. After publishing, each developer or operator must provide their own `appsettings.json` (and optional override) beside the application before running it.

Do not distribute existing `bin/Debug`, `bin/Release`, or other generated output folders without inspecting them: earlier or local builds may contain copied personal credentials. The publish exclusion does not remove configuration files already present in generated folders.

---

## Versioning & Release Process

This project follows Semantic Versioning (x.y.z).

- All pull requests into `main` require:
  - A successful CI build
  - A version bump in the primary project file
- Git tags mark production-ready releases  
- Release builds are archived for rollback safety  

---
