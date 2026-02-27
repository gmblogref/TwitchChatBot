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

The repository includes an `appsettings.example.json` file containing configuration keys only.

⚠️ This file does **not** contain any secrets.

To run the application:

1. Copy the template config:
   - Rename `TwitchChatBot/appsettings.example.json` to `TwitchChatBot/appsettings.json`
2. Fill in your values in `appsettings.json` (local only).
   - **Do not commit this file.** It is ignored by `.gitignore`.
3. Populate required values such as:
   - Twitch Client ID
   - Access Tokens
   - EventSub configuration
   - Media paths
4. Do **not** commit any real tokens or credentials.
5. (Recommended) Create an override file for secrets:
   - Create `TwitchChatBot/appsettings.Development.json`
   - Put sensitive values (tokens/secrets) there.
   - This file is also ignored by `.gitignore`.

The application loads configuration in this order:
- `appsettings.json`
- `appsettings.Development.json` (overrides base values if present)

Sensitive configuration files are excluded via `.gitignore`.

---

## Versioning & Release Process

This project follows Semantic Versioning (x.y.z).

- All pull requests into `main` require:
  - A successful CI build
  - A version bump in the primary project file
- Git tags mark production-ready releases  
- Release builds are archived for rollback safety  

---
