# AGENTS.md

## General Rules
- Do NOT modify production code unless explicitly instructed.
- Do NOT rename existing classes, methods, or files.
- Do NOT change method signatures.

## Test Generation Rules
- Create tests ONLY in TwitchChatBot.Tests.
- Follow the exact structure used in WheelServiceTests.
- Use xUnit, Moq, and FluentAssertions.
- Use Arrange / Act / Assert pattern.
- One behavior per test.
- Method names must follow:
  MethodName_Should_DoExpectedBehavior_When_Scenario

## Coding Style
- Place class fields at the top of the class.
- Use explicit types when clarity is needed.
- Put all IF statements in curly braces.

## Scope Control
- Do NOT create integration tests unless explicitly requested.
- Do NOT create UI tests.
- Do NOT modify .csproj files.
- Do NOT add new NuGet packages.

## Safety
- Do NOT stage, commit, or push code.
- Only generate or modify files explicitly requested.