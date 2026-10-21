# Media, Mapping, Overlay, and Windows Environment Setup

This guide documents the media and local-environment behavior implemented by the current TwitchChatBot source. It describes the existing JSON repositories, alert handlers, embedded web host, browser pages, Wheel integration, and Testing tab. It does not define a new runtime layout or automatically install sample data.

For application-setting precedence and the complete key inventory, see [Configuration Reference](CONFIGURATION.md). The seven sanitized templates under [`examples/media`](examples/media/) are teaching examples only. Copy them to private locations of your choice and point `MediaMapFiles:*` at those private copies.

## Windows and Runtime Requirements

The application has these verified environment requirements:

- Windows-only Windows Forms UI targeting `net10.0-windows`.
- .NET SDK `10.0.402`, selected through the repository-root `global.json`, for development.
- Windows 11 and Visual Studio 2026 with the **.NET desktop development** workload are the supported development baseline.
- Framework-dependent execution requires the matching .NET 10 Windows Desktop and ASP.NET Core runtimes.
- The solution uses Debug and Release `Any CPU` configurations. It does not declare an x64-only runtime identifier or platform target.
- The process must be able to read its configuration, JSON data, and media; write mutable JSON and Polly MP3 files; and bind the configured local web-host address.

Media playback uses HTML elements in a browser or OBS Browser Source. VLC, FFmpeg, WebView2, and the Windows Media Player control are not application requirements.

Live features additionally depend on the services they use:

- Twitch for chat, EventSub, Helix, and Twitch-originated alerts.
- AWS Polly for speech. See [AWS Polly Text-to-Speech Setup](AWS_POLLY_SETUP.md).
- Streamlabs for optional donation events. See [Streamlabs Donation Setup](STREAMLABS_SETUP.md).
- OpenAI for optional generated alert speech. See [OpenAI Alert-Text Setup](OPENAI_SETUP.md).
- OBS is optional; an ordinary browser can load the local overlay pages.

## Runtime Locations

Three different path bases are involved. Do not treat them as interchangeable.

| Content | Configuration/source | Resolution base |
| --- | --- | --- |
| `appsettings.json` and optional `appsettings.Development.json` | `Program.BuildConfiguration()` | `AppContext.BaseDirectory`, normally the directory containing the executable |
| Seven `MediaMapFiles:*` JSON paths | `DataHelperMethods.Get*Path()` | `Path.GetFullPath` against the process current working directory when the configured value is relative |
| `WebHost:WebRoot` and relative overlay files | Embedded ASP.NET Core host | Host/process working context |
| Media referenced inside a mapping | `MediaBase:TwitchAlertsFolder` plus the mapped relative value | Physical media root exposed as `/media` |

Use absolute paths for the seven `MediaMapFiles:*` settings when practical. Use relative paths inside the mapping files, such as `twitch_alerts/follow.mp4`. An absolute file path inside a media mapping may pass the local existence check but becomes an unusable `/media/C:/...` browser URL. HTTP URLs inside normal media mappings are also not supported by this path pipeline.

Raid profile images are different: the Twitch image URL is sent separately as alert metadata and assigned directly to an HTML image element.

### Debug and Release working directories

Visual Studio commonly starts the application with the UI project directory as the working directory. An executable opened from `bin\Release\net10.0-windows` commonly starts with that output directory as its working directory. A shortcut, script, terminal, or scheduler can choose another working directory.

Consequences:

- Configuration still loads beside the executable because it uses `AppContext.BaseDirectory`.
- Relative `MediaMapFiles:*` values can resolve to different files between Visual Studio and a Release executable.
- A relative `wwwroot` works only when the selected working/host context contains the expected copied directory.
- Absolute private JSON paths avoid the first difference. Keep the copied `wwwroot` directory with Release output.

## Media Directory Structure

The repository tracks only the browser assets:

```text
TwitchChatBot/
└── wwwroot/
    ├── alerts.html
    ├── fullscreen.html
    ├── stylealerts.css
    └── stylefullscreen.css
```

The repository does not currently track a `MediaMaps` directory, category media directories, example media, or private runtime JSON.

A private media root compatible with the supplied examples can look like this:

```text
<private-media-root>/
├── channel_points/
├── commands/
├── donation_alerts/
├── first_chat_vods/
├── text_to_speech/
└── twitch_alerts/
```

These category names are conventions used by the examples, not hard-coded directory requirements. A mapping can use another relative subdirectory as long as it exists beneath the configured media root and its value resolves to the intended file.

### Automatic versus developer-supplied directories

`TtsService` calls `Directory.CreateDirectory` for `<MediaBase:TwitchAlertsFolder>\text_to_speech` when the dependency graph creates the service. This can also create missing parent directories when permissions allow.

The application does not create or populate the other media directories. Developers must supply:

- Media files referenced by their mappings.
- Any category directories used by those relative paths.
- Private mapping copies for read-only maps.
- A valid `appsettings.json` beside the executable.

The shared JSON save helper creates a mapping file's parent directory when a repository performs a save. Loading a missing mapping does **not** write a default file, despite the loader's “Creating new” log text.

## Build and Publish Copy Behavior

The UI project copies all tracked `wwwroot/**/*.*` content into Debug and Release output. It does not copy the external media root or private mapping files.

Local builds always copy private `appsettings.json` and `appsettings.Development.json` when those files exist in the project directory. Publish output excludes them and includes `appsettings.example.json`. Inspect build output before sharing it because local output can contain credentials and other private configuration.

## Configure the Seven JSON Paths

All seven setting strings are required by startup validation, even when the corresponding file or optional provider is not currently used:

```json
{
  "MediaMapFiles": {
    "TwitchAlertMedia": "C:\\TwitchChatBotData\\twitchAlertsMediaMap.json",
    "ExcludedUsersMedia": "C:\\TwitchChatBotData\\excludedUsers.json",
    "FirstChattersMedia": "C:\\TwitchChatBotData\\firstChatMediaMap_v2.json",
    "CommandAlertMedia": "C:\\TwitchChatBotData\\commandsMediaMap.json",
    "UserWatchStreakMedia": "C:\\TwitchChatBotData\\watch_streaks_v2.json",
    "WheelMedia": "C:\\TwitchChatBotData\\wheelsMediaMap.json",
    "DonationMedia": "C:\\TwitchChatBotData\\donationMediaMap.json"
  },
  "MediaBase": {
    "TwitchAlertsFolder": "C:\\TwitchChatBotMedia"
  }
}
```

The filenames are configurable. The template filenames below are descriptive examples, not names hard-coded into a repository. Do not point runtime configuration directly into `docs/examples/media`; copy the desired template to a private data location first.

All repository loaders accept property names case-insensitively, skip JSON comments, allow trailing commas, and deserialize enums through `JsonStringEnumConverter`. The examples use the canonical property spelling produced or explicitly named by the current models.

## Mapping and State File Reference

### Twitch alert media

- Setting: `MediaMapFiles:TwitchAlertMedia`
- Template: [`twitchAlertsMediaMap.example.json`](examples/media/twitchAlertsMediaMap.example.json)
- Model: `TwitchAlertMediaMap`
- UI editor: none
- Runtime persistence: none
- Reload: cached after first successful/default load; restart after manual edits

Top-level properties:

| Property | Type | Selection/use |
| --- | --- | --- |
| `channel_points` | object with `tiers` array and `default` string | Tier `title` is matched case-insensitively. The current handler does not use `default`. |
| `channel_points_text` | object with `tiers` array | Tier `title` selects text; `[userName]` is replaced in `message` and optional `ttsMessage`. |
| `cheer` | object with `tiers` array and `default` string | Bits 38 and 350 seek exact `min`; other values select the greatest eligible `min`; otherwise `default`. |
| `follow` | string array | Random entry. |
| `hype_train` | string array | Random entry for Hype Train begin. |
| `raid` | string array | Random entry. |
| `resub` | string array | Random entry. |
| `subgift` | string array | Random entry for an individual gift. |
| `submysterygift` | object with `tiers` array and `default` string | First tier whose `match` is `eq` and count equals `value`, or whose `match` is `gte` and count is at least `value`; otherwise `default`. |
| `subscription` | string array | Random entry. |
| `watch_streak` | string array | Random entry. |

The shared tier model can contain:

- `title`: string reward key.
- `media`: string relative media path.
- `message`: string displayed for a text reward.
- `ttsMessage`: optional string spoken for a text reward.
- `min`: nullable integer threshold.
- `match`: string; only `eq` and `gte` are implemented for mystery gifts.
- `value`: nullable integer used with `match`.

There is no property-level schema validation. Include the fields required by the category being configured and omit unrelated optional tier fields.

### Excluded users

- Setting: `MediaMapFiles:ExcludedUsersMedia`
- Template: [`excludedUsers.example.json`](examples/media/excludedUsers.example.json)
- Shape: JSON array of username strings
- UI editor: none
- Runtime persistence: none
- Reload: a nonempty set is cached; an empty set is loaded again on later checks

Matches are case-insensitive. A missing or invalid file becomes an empty set, which means no user is excluded.

### First Chatter media

- Setting: `MediaMapFiles:FirstChattersMedia`
- Template: [`firstChatMediaMap_v2.example.json`](examples/media/firstChatMediaMap_v2.example.json)
- Root property: `FirstChatterMediaItems`, an array
- UI editor: none
- Runtime persistence: user-ID backfills and username changes can be saved
- Reload: loaded once; restart after manual edits

Each item contains:

| Property | Type | Use |
| --- | --- | --- |
| `UserId` | string | Preferred stable match key. |
| `CurrentUserName` | string | Fallback legacy match and display identity. |
| `Media` | string | Relative media path. |
| `AllowedDaysOfWeek` | array of `DayOfWeek` | Empty or null means any day; valid names are Sunday through Saturday. |

The repository contains an intended legacy `{ "username": "media-path" }` migration. Because the new model initializes its item list, legacy JSON may deserialize as an empty new-format object before the migration fallback is reached. Do not rely on automatic legacy migration; use the current object shape.

If a matching usable item is absent or its day restriction fails, First Chatter sends its normal welcome message to Twitch chat instead of displaying media.

### Command mappings

- Setting: `MediaMapFiles:CommandAlertMedia`
- Template: [`commandsMediaMap.example.json`](examples/media/commandsMediaMap.example.json)
- Root property: `commandMediaItems`, an array
- UI editor: none
- Runtime persistence: none
- Reload: populated maps are cached; an empty map can be read again on later lookups

Each item contains `Command` plus optional `Text` and `Media` strings. Command lookup is case-insensitive. An entry can send Twitch chat text, enqueue media, or do both. A missing command entry logs an unknown-command debug message and performs no fallback alert.

### Watch-streak state

- Setting: `MediaMapFiles:UserWatchStreakMedia`
- Template: [`watch_streaks_v2.example.json`](examples/media/watch_streaks_v2.example.json)
- Model: `WatchStreakState`
- UI editor: none
- Runtime persistence: yes; writes are debounced and flushed during shutdown
- Reload: cached state

Root properties are integer `Version`, integer `CurrentStreamIndex`, and a `Users` object keyed by identity. Each user record contains `UserId`, `UserName`, `FailedUserIdLookups`, `TotalStreams`, `Consecutive`, `LastAttendedIndex`, and nullable `LastSeenUtc`/`FirstSeenUtc` timestamps.

This file is runtime-managed and generally should not be manually edited. The repository normalizes legacy data, can re-key username records to user IDs, and may overwrite manual changes with in-memory state. Back it up before any exceptional repair.

This state file is distinct from the `watch_streak` media array in the Twitch alert map.

### Wheel definitions

- Setting: `MediaMapFiles:WheelMedia`
- Template: [`wheelsMediaMap.example.json`](examples/media/wheelsMediaMap.example.json)
- Shape: array of wheel objects
- UI editor: Wheel tab
- Runtime persistence: yes
- Reload: repository reads the file during wheel queries; UI saves replace the stored list

A wheel contains `Id`, `Name`, and an `Items` array. Each item contains:

- `Id`: string identity.
- `Position`: integer ordering value.
- `DisplayName`: result/segment text.
- `AlertType`: nullable string.
- `AlertKey`: nullable string.
- `Weight`: integer; UI requires a value greater than zero, while runtime selection defensively treats nonpositive values as `1`.
- `IsHidden`: Boolean; hidden items are excluded from selection.

The Wheel UI manages wheel names and the item display name, weight, hidden state, order, creation, and removal. The current grid does **not** expose `AlertType` or `AlertKey`. Existing values are preserved when existing rows are saved, while items created in the UI have these fields blank unless the private JSON is edited separately.

Implemented dispatch values:

| `AlertType` | Meaning of `AlertKey` |
| --- | --- |
| `channelpoints` | A `channel_points` or `channel_points_text` tier `title`. |
| `commands` | A command key such as `!celebrate`. |

Type matching is case-insensitive. Blank type, a missing required key, or an unknown type produces a generic text-only alert containing `DisplayName`. A valid type with a nonexistent channel-point or command key produces no mapped alert and does not use the generic fallback. Dispatch exceptions are caught and use the generic text-only fallback.

After weighted selection and animation, the result is shown in a panel inside the main form. Dispatch occurs only when the user presses **OK**, and the panel is hidden after the dispatch call returns. There is no Wheel-specific media category or mapping pipeline.

### Donation mappings

- Setting: `MediaMapFiles:DonationMedia`
- Template: [`donationMediaMap.example.json`](examples/media/donationMediaMap.example.json)
- Root properties: `default` string and `tiers` array
- UI editor: none
- Runtime persistence: none
- Reload: a nonempty/defaulted map is cached; an entirely empty map can be read again

Each tier contains decimal `Min` and string `Media`. The handler selects the tier with the greatest `Min` not greater than the donation amount, then falls back to `default`. If neither supplies media, the donation still enqueues a text-only visual alert and requests speech.

## Implemented Alert Categories

| Category | Event source | Mapping/behavior | Speech | Testing tab |
| --- | --- | --- | --- | --- |
| Follow | EventSub | Random `follow` entry | Yes | Yes |
| Subscription | TwitchLib chat event | Random `subscription` entry | Yes | Yes |
| Resubscription | TwitchLib chat event | Random `resub` entry | Yes/optional AI | Yes |
| Individual gift subscription | TwitchLib chat event | Random `subgift` entry | Yes | Yes |
| Community/mystery gift | TwitchLib chat event | `submysterygift` tier/default | Yes/optional AI | Yes |
| Raid | TwitchLib raid notification | Random `raid` entry plus optional remote profile image | Yes/optional AI | Yes |
| Cheer/bits | EventSub | `cheer` threshold/default | Yes | Yes |
| Channel-point redemption | EventSub through redemption orchestration | Media tier or text tier by title | Optional for text rewards | Yes |
| Command | Twitch chat | Command text and/or media | Command-specific paths | Yes |
| First Chatter | Twitch chat | Per-user/day item; Twitch-chat fallback | No separate TTS | Yes |
| Watch streak alert | IRC `USERNOTICE` | Random `watch_streak` entry | Yes/optional AI | Yes |
| Hype Train begin | EventSub | Random `hype_train` entry | No | Yes |
| Hype Train end | EventSub | No media mapping; speech summary | Yes | Yes |
| Donation | Streamlabs Socket.IO | Donation tier/default; text-only fallback | Yes | Yes |
| Wheel result | Wheel tab confirmation | Existing channel-point/command path or generic text | Inherited | No dedicated test |

Hype Train progress is subscribed but has no notification handler. The `fullscreen` browser page exists, but no current production handler was found that enqueues an alert whose type is `fullscreen`.

## Supported Media and Playback

The application explicitly selects renderers by path text in the browser pages:

| Extension | Explicit behavior |
| --- | --- |
| `.mp4` | HTML `<video>` with autoplay, sound enabled, and a 30-second completion timeout. |
| `.gif` | HTML `<img>` displayed for five seconds. Animated GIF behavior is delegated to the browser. |
| `.mp3` | HTML `Audio` with a 30-second completion timeout. |

The alert page lowercases the path and checks whether it contains the extension. The fullscreen page uses case-sensitive `endsWith`. Prefer lowercase extensions without URL query text.

PNG, JPEG, BMP, WAV, WebM, AVI, MOV, MKV, SVG, playlists, and remote URLs are not explicitly handled as mapped alert media. Do not claim support merely because a particular Windows/browser installation might decode them.

MP4 and MP3 container recognition does not guarantee codec support. Decoding is delegated to the browser engine used by the browser or OBS. When `play()` fails, the page suppresses the rejected promise and waits for a media error or the 30-second timeout. GIF load errors have no special handler; the five-second interval still elapses.

An existing file with an unsupported extension passes the server existence check but enters the browser's generic delay branch and is not rendered. A media-only alert can therefore appear to do nothing.

## Media Existence and Alert Dropping

Before enqueueing any alert that has `MediaPath`, `AlertService` converts it to a physical path and calls `File.Exists`. If that check is false, the service logs a warning and returns without enqueueing the alert.

This drops the **entire alert**, including its text. It does not strip the missing media and retry as text-only content. Speech requested separately by an event handler can still occur because TTS has its own queue, but that varies by category and execution order.

Category-specific empty-map behavior also varies:

- Follow checks for a missing/empty list, logs, and returns before visual alert and speech.
- Subscription, resubscription, gift, raid, Hype Train begin, and watch-streak paths index random lists without a consistent empty-list guard; an empty list can throw and abort that event handler.
- Cheer can use `default`; an empty media string suppresses its visual alert while its speech path continues.
- Mystery gifts can use `default`; an empty media string suppresses the visual alert while speech handling continues.
- Channel points with no matching media/text title add history but display nothing.
- Commands with no matching key do nothing.
- First Chatter with no usable mapping sends its normal Twitch chat welcome message.
- Donations with no selected media enqueue their message as a text-only alert.
- Wheel blank/unknown dispatch metadata uses a generic text-only result, but a valid type with a missing target key does not.

## Missing and Invalid JSON Behavior

The seven path **settings** are required and must be nonblank. The seven data **files** are not checked during application startup.

The shared loader behaves as follows:

| Condition | Result |
| --- | --- |
| File missing | Logs the condition and returns an empty model/set/list in memory. No file is written. |
| Malformed JSON, wrong root type, access error, or invalid enum | Logs an error and returns the file's empty default model. |
| JSON literal `null` | Logs a warning and returns the empty default. |
| Unknown property | Ignored by deserialization. |
| Omitted ordinary property | Usually receives the model's initialized/default value. |
| Explicit `null` for a collection/object | Can survive deserialization and cause later null-reference failures; there is no general schema validator. |
| Semantically invalid entry | May be skipped, unmatched, selected incorrectly, or throw later depending on the handler. |

File-specific persistence and reload behavior is listed in each file section. In general, restart after manually editing cached read-only maps. Do not manually edit watch state while the application is running. Use the Wheel UI for UI-managed fields, and stop the application before exceptional direct edits to Wheel dispatch metadata.

## Overlay and OBS Setup

Starting the bot runtime starts the embedded ASP.NET Core host. With the default base URL, the relevant pages are:

```text
http://localhost:3000/alerts
http://localhost:3000/fullscreen
```

Configure `/alerts` as an OBS **Browser Source** for normal application alerts. The page stays open and receives queued alerts over WebSocket; individual media/text elements are inserted temporarily and removed when complete.

Current limitations:

- Both overlay styles use a solid black body background. They do not implement transparent CSS or a chroma/transparency key.
- There is no borderless/topmost WinForms alert popup.
- The fullscreen page listens only for `fullscreen` alert payloads, and no current production handler emits that type.
- The Wheel result appears in a panel within the main `Twitch Chat Bot` window. It is not a temporary result form.
- Closing or refreshing a browser source disconnects its WebSocket. The main Windows Forms application remains open.

The historical concern that OBS Window Capture could switch to the main application after a temporary result popup closes is not represented by the current Wheel implementation. Capture the browser URL for alerts rather than relying on Window Capture of transient dialogs.

## Alert Queue and Offline Behavior

Alerts enter one server-side queue. Browser clients acknowledge receipt and send a completion message after playback/display.

- If the queue reaches `AlertSettings:MaxQueueSize`, the oldest queued alert is dropped.
- If no overlay client is connected, the current alert is requeued and processing pauses.
- After `AlertSettings:MaxOfflineQueueTime`, alerts held for an offline overlay are dropped to prevent backlog.
- If acknowledgement does not arrive within `AlertSettings:AcknowledgeTimeOut`, the alert is held/retried subject to offline expiry.
- After acknowledgement, `AlertSettings:AlertTimeOut` forcibly completes a stuck alert.
- Browser MP4/MP3 handling has its own 30-second timeout before sending completion.
- The browser pages retain at most one local `pending` alert while already playing; the server queue remains the primary sequencing mechanism.

Keep an OBS/browser overlay connected before triggering tests. A successful application enqueue does not guarantee that an offline overlay displayed the alert.

## First-Run Setup

1. Install the verified Windows/.NET development or runtime prerequisites.
2. Copy `TwitchChatBot/appsettings.example.json` to the ignored `TwitchChatBot/appsettings.json` and replace placeholders.
3. Create a private JSON-data directory outside the repository when practical.
4. Copy each desired file from [`docs/examples/media`](examples/media/) to that private directory, removing `.example` from the copied filename if desired.
5. Configure all seven `MediaMapFiles:*` paths. The names can differ as long as each setting points to the correct schema.
6. Create a private media root and the subdirectories referenced by your copied mappings.
7. Add your own media files. The repository deliberately contains none.
8. Configure `MediaBase:TwitchAlertsFolder` to the physical media root.
9. Retain `wwwroot` beside the executable in build output.
10. Complete Twitch and any optional provider setup through their dedicated guides.
11. Start the application. A configuration error before the main window indicates a missing/malformed application setting, not a missing media map.
12. Start the bot runtime so the embedded host and overlay pipeline run.
13. Open `/alerts` in a browser or OBS Browser Source before triggering an alert.
14. Test with fictional or controlled inputs and watch application logs.

Do not expect first-run loading to save missing example maps automatically.

## Testing Tab Safety

The Testing tab is a manual production-path utility, not an isolated test environment. Its buttons call the same singleton services, mappings, queues, history, Twitch client, and Polly service used during normal operation.

Potential side effects include:

- Sending real Twitch chat from command tests.
- Sending the First Chatter fallback welcome message to the configured Twitch channel.
- Reaching configured special-command, moderation, or Nuke behavior.
- Adding entries to the live in-memory alert history.
- Updating and saving First Chatter identity information.
- Reading and displaying real configured media.
- Calling AWS Polly, creating temporary MP3 files, and incurring AWS charges.
- Exercising optional OpenAI speech generation from qualifying alert tests.
- Playing alerts to connected browser/OBS sources.

The Test Bot flag protects watch-streak state updates in several service paths, but it does not isolate every Testing-tab action. Back up mutable JSON, use test identities/data, verify the configured Twitch channel, and understand provider costs before using the tab.

## Troubleshooting

| Symptom | Checks and current behavior |
| --- | --- |
| Application exits before the main window | Confirm `appsettings.json` is beside the executable, is valid JSON, and contains every startup-required setting. Mapping files are not the startup check. |
| Mapping path works in Visual Studio but not Release | Replace relative `MediaMapFiles:*` paths with absolute private paths and confirm the process working directory. |
| Bot starts but overlay URL fails | Confirm `WebHost:BaseUrl`, port availability, `WebHost:WebRoot`, and copied `wwwroot` content. |
| Overlay opens but receives nothing | Start the bot runtime, confirm the WebSocket client is connected, and review queue/offline logs. |
| Alert text and media both disappear | Check the physical media path. A failed `File.Exists` check drops the whole alert. |
| Unsupported media appears to pause | Only MP4, GIF, and MP3 have explicit renderers. Other existing extensions enter a delay branch. |
| MP4 or MP3 times out | Confirm the file is reachable through `/media`, uses a browser-supported codec, and can autoplay with sound in the selected browser/OBS engine. |
| GIF does not animate/display | Confirm the path and file, and verify it loads directly through `/media/...`; image errors are not surfaced to the application. |
| No follow alert or speech | A missing/empty `follow` list returns before both paths. |
| Donation shows text but no media | No eligible tier/default media was selected, or the configured value is blank. This text-only fallback is donation-specific. |
| Channel-point redemption does nothing | Match the Twitch reward title to a media or text tier `title`, including spacing; matching is case-insensitive. The modeled media `default` is not used. |
| Wheel result does nothing after OK | A valid `AlertType` may point to a missing `AlertKey`; that path has no generic fallback. |
| Manual edit is not observed | Restart for cached maps. Stop the application before editing runtime-managed state. |
| JSON is reported invalid | Validate root type, property value types, enum day names, commas, and quotes. One deserialization error replaces the whole loaded model with its empty default. |
| Polly MP3 cannot be created | Confirm the configured media root is writable and review [AWS Polly Setup](AWS_POLLY_SETUP.md). |

## Template Index

| Template | Purpose |
| --- | --- |
| [`twitchAlertsMediaMap.example.json`](examples/media/twitchAlertsMediaMap.example.json) | Twitch alert, channel-point, cheer, Hype Train, subscription, raid, and watch-streak media definitions. |
| [`excludedUsers.example.json`](examples/media/excludedUsers.example.json) | Case-insensitive excluded-user list. |
| [`firstChatMediaMap_v2.example.json`](examples/media/firstChatMediaMap_v2.example.json) | Per-user First Chatter media and optional day restrictions. |
| [`commandsMediaMap.example.json`](examples/media/commandsMediaMap.example.json) | Command text and/or media definitions. |
| [`watch_streaks_v2.example.json`](examples/media/watch_streaks_v2.example.json) | Runtime-managed watch-streak state shape. |
| [`wheelsMediaMap.example.json`](examples/media/wheelsMediaMap.example.json) | Wheel definitions, weights, visibility, and existing alert dispatch metadata. |
| [`donationMediaMap.example.json`](examples/media/donationMediaMap.example.json) | Default and threshold-based donation media. |

These templates contain fictional identities and filenames. They contain no credentials, private paths, or actual media, and the application does not load them unless a developer explicitly copies and configures private runtime files.

## Related Documentation

- [README](../README.md)
- [Configuration Reference](CONFIGURATION.md)
- [Twitch Developer and OAuth Setup](TWITCH_SETUP.md)
- [Streamlabs Donation Setup](STREAMLABS_SETUP.md)
- [OpenAI Alert-Text Setup](OPENAI_SETUP.md)
- [AWS Polly Text-to-Speech Setup](AWS_POLLY_SETUP.md)
- [Feature History](../FEATURE_HISTORY.md)
