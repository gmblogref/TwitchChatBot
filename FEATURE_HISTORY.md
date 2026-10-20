# Feature History

## Story 1.2.1 — .NET Framework & NuGet Dependency Modernization

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

- Corrected two .NET 10 WFO1000 compilation errors in `WheelSpinnerControl.cs` on `Items` and `RotationAngle`, which were caused by the public control properties lacking explicit designer code-serialization metadata.
- Marked the runtime-populated `Items` property with `DesignerSerializationVisibility.Hidden`.
- Declared the `RotationAngle` default with `DefaultValue(0f)`.
- The attributes affect design-time serialization only; wheel rendering, rotation, item loading, and spin animation behavior remain unchanged.
- The Visual Studio 2026 WinForms designer was manually opened successfully after the correction.
- No designer-generated source changes were necessary.

### GitHub Actions and Version

- Updated the build workflow to use `actions/checkout@v7` and `actions/setup-dotnet@v6`.
- The build workflow selects the SDK through the repository `global.json` with GA SDK quality.
- Updated the version-check workflow to use `actions/checkout@v7` without changing its validation behavior.
- The existing build and version-check workflow behavior was otherwise preserved.
- Both mandatory GitHub Actions checks remain pending validation after pull-request creation; they have not yet been reported as passed.

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
- GitHub Actions validation remains pending pull-request execution.

### Acceptance Status

**IMPLEMENTATION COMPLETE — DEVELOPER VALIDATED**

**PR / GITHUB ACTIONS VERIFICATION PENDING**
