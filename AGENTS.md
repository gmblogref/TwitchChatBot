# AGENTS.md

PROJECT_ID: TWITCH-CHATBOT

## Project Identity Guard v1.0

- Before modifying the repository for a formal handoff, verify that the handoff's `PROJECT_ID` exactly matches `TWITCH-CHATBOT`.
- If the project identity is missing or does not match, stop before modifying files and report the mismatch to the developer.

## General Rules

- Do not modify production code unless explicitly instructed.
- Do not rename existing classes, methods, or files.
- Do not change method signatures without explicit approval.
- Keep changes small, focused, and limited to the approved story.
- Do not perform unrelated refactoring.

## Development Baseline

- Target .NET 10 LTS.
- Use Visual Studio 2026 as the supported IDE baseline.
- This is a Windows-only Windows Forms application; keep the UI on a Windows-specific target framework.
- Use Windows 11 as the recommended development operating system.
- Select the .NET SDK through the repository-root `global.json`.
- The current selected SDK is 10.0.402 with `latestPatch` roll-forward and prerelease SDKs disabled.

The solution contains six projects:

- `TwitchChatBot.UI` (`TwitchChatBot/TwitchChatBot.UI.csproj`): Windows Forms UI, application entry point, dependency-injection composition, EventSub and Streamlabs provider implementations, embedded ASP.NET Core host, and browser-overlay assets.
- `TwitchChatBot.Core`: business logic, controllers, service contracts, alert orchestration, Twitch client integration, TTS, and shared runtime utilities.
- `TwitchChatBot.Data`: JSON-backed repositories and persistence helpers.
- `TwitchChatBot.Models`: shared models and configuration accessors.
- `TwitchChatBot.Testing`: application testing utilities used by the UI Testing tab; it is not the automated test project.
- `TwitchChatBot.Tests`: xUnit unit tests for Core, Data, and Models behavior.

## Architecture

- Keep Windows Forms presentation and application composition in `TwitchChatBot.UI`.
- Keep business rules and service contracts in `TwitchChatBot.Core`.
- Keep persistence implementations in `TwitchChatBot.Data`.
- Keep cross-project data structures and configuration access in `TwitchChatBot.Models`.
- Keep manual in-application test utilities in `TwitchChatBot.Testing`.
- Keep automated unit tests in `TwitchChatBot.Tests`.
- Preserve the existing dependency-injection and project-reference boundaries unless an approved architecture story changes them.

## Coding Conventions

- Place class fields and properties at the top of the class.
- Use explicit types when clarity is needed.
- Put all `if` statement bodies in curly braces.
- Do not rename existing classes, methods, or files as incidental cleanup.
- Do not change method signatures without explicit approval.

## Test Rules

- Create tests only in `TwitchChatBot.Tests`.
- Do not create integration tests unless explicitly requested.
- Do not create UI tests.
- Follow the structure used in `WheelServiceTests`.
- Use xUnit, Moq, and FluentAssertions.
- Use the Arrange / Act / Assert pattern.
- Test one behavior per test.
- Name test methods `MethodName_Should_DoExpectedBehavior_When_Scenario`.
- Do not claim tests passed unless the developer or an authoritative CI result confirms execution.

## Scope Control

- Do not modify `.csproj` files without explicit developer approval.
- Do not add or upgrade NuGet packages without explicit approval.
- Do not introduce new architectural rules or change project boundaries without architecture review.
- Modify only files authorized by the current story.

## Development Responsibilities

### User Story Controller

- Defines and prioritizes features.
- Maintains acceptance criteria and the backlog.
- Tracks completion.
- Provides dedicated story handoffs.

### Architecture Discussion

- Owns architectural decisions and boundaries.
- Maintains long-term technical direction.

### Dedicated Story Chats / Codex

- Investigate the approved story scope.
- Implement approved changes.
- Add targeted tests when appropriate and authorized.
- Report findings and exact changed files.
- Avoid unrelated modifications.

### Developer

- Owns branch creation and switching.
- Owns staging, commits, pushes, pull requests, and merges.
- Owns restore, builds, tests, and application execution.
- Reviews and accepts completed stories.

## GitHub Repository Rules

- `main` is protected.
- All integration into `main` requires a pull request.
- The developer owns all Git and pull-request operations.
- Do not stage, commit, push, pull, merge, create or switch branches, or create pull requests unless explicitly instructed by the developer.

Two GitHub Actions checks are required:

1. **Build**
   - Restores dependencies.
   - Builds the solution in Release configuration.
2. **Version bump check**
   - Applies to pull requests containing `.cs` or `.csproj` changes.
   - Requires the application version to be numerically greater than the version on `main`.
   - Prefer incrementing the patch component by one for ordinary development.
   - Higher minor or major versions are allowed when justified.
   - Documentation-only and workflow-only changes do not require an application version bump when no `.cs` or `.csproj` files changed.

## Security and Configuration

- Use `TwitchChatBot/appsettings.example.json` as the placeholder configuration template.
- Keep real credentials in ignored local configuration such as `appsettings.json` and `appsettings.Development.json`.
- Never commit credentials, OAuth tokens, API keys, or secrets.
- Preserve existing configuration validation unless a story explicitly changes it.
- Do not expose sensitive configuration values through logging, documentation, tests, or generated output.
- Preserve the existing local Release execution behavior.
- Local build output may contain copied private configuration files; never distribute build folders without inspecting and removing sensitive configuration.
- Published output excludes private configuration as currently implemented and includes the example template for operator setup.

## Documentation Maintenance

- Every completed implementation story must update `FEATURE_HISTORY.md` before pull-request merge.
- Review `AGENTS.md` when a major architectural or process change is accepted.
- Document behavior changes whenever implementation behavior changes.
- Do not duplicate extensive feature history in `AGENTS.md`.
- Treat `FEATURE_HISTORY.md` as historical documentation, not as the future-feature backlog.
- Distinguish implementation completion from developer validation and acceptance.
- Do not invent completion dates, test results, pull-request references, or validation outcomes.

## Safety

- Only generate or modify files explicitly requested.
- The developer owns restore, build, test, application execution, and all Git operations.
