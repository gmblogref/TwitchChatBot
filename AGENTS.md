# AGENTS.md

## General Rules
- Do NOT modify production code unless explicitly instructed.
- Do NOT rename existing classes, methods, or files.
- Do NOT change method signatures.

## Development Baseline
- Target .NET 10 LTS.
- Use Visual Studio 2026 as the supported IDE baseline.
- Select the .NET SDK through the repository-root `global.json`.
- Use Windows 11 as the recommended development operating system.
- Keep the Windows Forms UI on a Windows-specific target framework.

## Test Generation Rules
- Create tests ONLY in TwitchChatBot.Tests.
- Follow the exact structure used in WheelServiceTests.
- Use xUnit, Moq, and FluentAssertions.
- Use Arrange / Act / Assert pattern.
- One behavior per test.
- Method names must follow:
  MethodName_Should_DoExpectedBehavior_When_Scenario

## Coding Style
- Place class fields and properties at the top of the class.
- Use explicit types when clarity is needed.
- Put all IF statements in curly braces.

## Scope Control
- Do NOT create integration tests unless explicitly requested.
- Do NOT create UI tests.
- Do NOT modify .csproj files without explicit developer approval.
- Do NOT add new NuGet packages.

## Repository Process
- The developer owns restore, build, test, and application execution.
- The developer owns all Git and pull-request operations.
- Integrate through pull requests only; `main` remains protected.
- Update `FEATURE_HISTORY.md` for accepted stories.

## Safety
- Do NOT stage, commit, or push code.
- Only generate or modify files explicitly requested.
