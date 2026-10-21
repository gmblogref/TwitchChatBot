# Architecture Guide

This guide describes the architecture implemented by the current TwitchChatBot source. It is intended for contributors who need to understand project boundaries, runtime composition, external integrations, persistence, and known limitations before changing the application.

For operator setup, use the [README](../README.md) and the focused guides for [configuration](CONFIGURATION.md), [Twitch](TWITCH_SETUP.md), [media and overlays](MEDIA_ENVIRONMENT_SETUP.md), [Streamlabs](STREAMLABS_SETUP.md), [OpenAI](OPENAI_SETUP.md), and [AWS Polly](AWS_POLLY_SETUP.md). Testing and manual acceptance procedures are in the [Testing Guide](TESTING.md).

## Solution Structure and References

The solution contains six projects.

| Project | Target | Responsibility | Direct project references |
| --- | --- | --- | --- |
| `TwitchChatBot.UI` (`TwitchChatBot/TwitchChatBot.UI.csproj`) | `net10.0-windows` | WinForms entry point and UI, dependency-injection composition, embedded ASP.NET Core host, EventSub and Streamlabs provider implementations, Wheel rendering, and browser assets | Core, Data, Models, Testing |
| `TwitchChatBot.Core` | `net10.0` | Controllers, business services and contracts, TwitchLib chat integration, alerts, TTS, Helix access, Watch Streaks, and Wheel selection/dispatch | Data, Models |
| `TwitchChatBot.Data` | `net10.0` | JSON repositories, path resolution, serialization, and atomic file replacement | Models |
| `TwitchChatBot.Models` | `net10.0` | Shared models and the static configuration accessor/validator | None |
| `TwitchChatBot.Testing` | `net10.0` | Manual Testing-tab utility that invokes production services | Core, Data, Models |
| `TwitchChatBot.Tests` | `net10.0` | xUnit automated unit tests for selected Core, Data, and Models behavior | Core, Data, Models |

The dependency direction is not a strict layered or ports-and-adapters design:

- Core directly references Data and therefore knows repository contracts defined in the persistence project.
- UI references every non-test runtime project and contains several infrastructure implementations for Core contracts.
- `Program.Main` is both the WinForms entry point and the composition root.
- `AppSettings.Configuration` is global static state used across projects.
- `ChatBotController` and `AlertService` expose setter-based UI bridges to avoid circular constructor dependencies. `Program.Main` currently wires only the controller bridge; alert delivery itself uses browser WebSockets.
- Wheel selection/dispatch is in Core, while animation, result presentation, editing, and confirmation remain in the main WinForms form.

These are descriptions of the current design, not new dependency rules.

## Process Entry and Composition

`Program.Main` performs application initialization in this order:

1. Initializes Windows Forms application settings.
2. Loads required `appsettings.json` and optional `appsettings.Development.json` from `AppContext.BaseDirectory`.
3. Assigns the resulting `IConfiguration` to `AppSettings.Configuration`.
4. Validates required strings, the allowed-voices collection, and required integers with their minimum values. It does not perform general URL/path or credential-validity checks. A missing or malformed file, or failed validation, displays a configuration dialog and exits before creating the form.
5. Registers repositories, services, named `HttpClient` instances, provider implementations, the main form, controller, and manual test utility as mostly singleton services. `IModerationService` is scoped, although it is resolved through singleton consumers in the root provider.
6. Builds the service provider, resolves the main form and `ChatBotController`, and manually supplies the form as the controller's `IUiBridge`.
7. Adds the text-box logger provider after the form exists.
8. Enters the WinForms message loop with `Application.Run`.

Configuration uses JSON providers only. The Development file overrides matching values from the base file, and both providers request reload-on-change. Environment-variable and command-line configuration providers are not added. See the [Configuration Reference](CONFIGURATION.md) for the authoritative key inventory and copy behavior.

Resolving the main form also constructs its dependencies. This starts some lifetime work before the user presses Start: `AlertService` creates its queue processor, and `TtsService` creates the speech directory and starts its credential-check/queue runner.

## Bot Lifecycle

The Start Bot and Test Bot buttons set `IAppFlags.IsTesting` and call `ChatBotController.StartAsync`. The controller prevents duplicate starts and performs these operations in order:

1. Starts the local overlay `WebSocketServer` keepalive loop.
2. Starts the embedded ASP.NET Core host and waits for the configured local listener to bind.
3. Wires Twitch chat/viewer events to the WinForms UI bridge.
4. Calls `TwitchClientWrapper.ConnectAsync` for TwitchLib/IRC.
5. Starts the EventSub background listener.
6. Starts the optional Streamlabs background service.
7. Begins the Watch Streak stream session. Live mode increments and schedules persistence of the stream index; Test Bot mode does not update Watch Streak statistics.
8. Starts the recurring internal ad-command timer.

`EventSubSocketService.StartAsync` and `StreamlabsDonationSocketService.StartAsync` schedule background work and return immediately. A completed `ChatBotController.StartAsync` therefore does **not** prove that EventSub has received `session_welcome`, subscriptions succeeded, or Streamlabs connected. External readiness must be confirmed from later logs and controlled behavior.

If a synchronous startup step throws, the controller logs the failure and does not rethrow it to the form. It resets its started flag before attempting `StopBotAsync`; because the stop method treats the controller as already stopped, that cleanup attempt can return without unwinding a partial start. The UI click handlers disable both start buttons after the awaited call, so logs are the authoritative indication of startup failure.

`StopBotAsync` is designed to stop the running session while leaving the WinForms process open. It attempts these operations in order, with each operation isolated by its own warning boundary:

1. Stop the ad timer.
2. End the Watch Streak session and flush pending saves.
3. Stop EventSub.
4. Stop Streamlabs.
5. Disconnect TwitchLib/IRC.
6. Unwire UI-facing Twitch events.
7. Stop overlay WebSockets.
8. Stop and dispose the embedded web host.

The Watch Streak end/flush operations are therefore an important use of Stop Bot, but they are not its only intended work.

The current source does not establish a reliably reusable stopped state across every integration:

- `TwitchClientWrapper.DisconnectAsync` asks TwitchLib to disconnect when connected, but it does not cancel or dispose the wrapper's reconnect timer or watchdog timer. Its TwitchLib `OnDisconnected` handler unconditionally starts the reconnect loop, and the watchdog also requests reconnect while disconnected. Those timers are disposed only by `TwitchClientWrapper.Dispose`/`DisposeAsync` during full application shutdown. Consequently, Twitch connection activity can continue or resume after Stop Bot even though the controller called `DisconnectAsync`.
- The controller unwires its own chat/viewer UI event bridge after disconnecting, but the TwitchLib handlers installed by `TwitchClientWrapper` remain attached for the lifetime of that wrapper.
- `EventSubSocketService.StopAsync` cancels its connection and lifetime tokens, closes its socket, and awaits the listener task. A later start creates a new listener lifetime, so the code is structured for reuse; this has not been established as reliable through the targeted Stop/Start runtime validation requested by the developer.
- `StreamlabsDonationSocketService.StopAsync` cancels and awaits its background task before disconnecting/disposing its socket. Its initial Socket.IO `ConnectAsync` call does not receive the service cancellation token, so an in-progress connection can delay observation of the stop request.
- `WebSocketServer.Stop` cancels but does not await its keepalive task, then removes and aborts overlay sockets. `WebHostWrapper.StopAsync` awaits host stop and disposes the host; a later start builds another host.
- If controller startup fails after partially starting dependencies, the controller resets its started flag before calling `StopBotAsync`; the guard can then make that cleanup call return immediately.

The developer has observed that Stop Bot does not consistently yield a reusable stopped state, Twitch may not reliably reconnect on a later Start, and background connection activity can end gradually. These observations are consistent with the lifecycle gaps above, but no targeted runtime investigation has established a single root cause or proven that reconnection is always impossible. Treat Stop/Start reuse as unverified.

Application shutdown first invokes the same stop path, then disposes TTS and Twitch. Twitch disposal cancels the wrapper's reconnect/watchdog timers; TTS disposal cancels its runner and best-effort deletes files in the configured `text_to_speech` directory. The form's asynchronous closing handler invokes this full shutdown before allowing the process to exit. Based on current developer operation, closing the application is the reliable way to end the session fully; it is not evidence that an in-process Stop/Start cycle is clean or reusable.

## Twitch Integration Boundaries

### TwitchLib and IRC

`TwitchClientWrapper` owns the current Twitch chat connection. It uses TwitchLib credentials, joins the configured channel, requests IRC tags/commands/membership capabilities, maintains reconnect and stale-activity watchdog timers, and publishes chat/viewer changes to `ChatBotController` for display.

IRC/TwitchLib currently handles:

- Incoming and outgoing channel chat.
- First Chatter detection and optional automatic shout-out processing.
- `!` command routing.
- joined/left viewer tracking and display grouping.
- new subscriptions, resubscriptions, individual gifts, community gifts, and raids.
- gift-bundle suppression for related individual gift events.
- raw `USERNOTICE` parsing for shared Watch Streak milestones.
- the internal recurring `!ads` command.

Migration of chat read/write behavior to the Twitch Chat API is future work; current functionality still depends on TwitchLib/IRC.

### Twitch Helix

Named Helix clients and Core services perform HTTP operations outside IRC:

- Resolve login-to-user ID mappings.
- Resolve current or last-known games for shout-out behavior.
- retrieve moderators and VIPs with pagination.
- retrieve raid profile-image URLs.
- resolve moderation targets and issue timeout requests.
- cancel/refund blocked channel-point redemptions when eligible.

Several lookup results are cached in memory. Helix failures are generally logged and converted to empty/null results or best-effort continuation, depending on the calling feature.

### EventSub WebSocket

`EventSubSocketService` owns an independent Twitch EventSub WebSocket. Its listener loop connects, receives `session_welcome`, then registers subscriptions over Helix using the current session ID. It handles Twitch-requested reconnect URLs, server keepalives/pings, connection failures, and bounded exponential reconnect delays.

The service registers:

- `channel.cheer`
- `channel.channel_points_custom_reward_redemption.add`
- `channel.channel_points_custom_reward_redemption.update`
- `channel.hype_train.begin`
- `channel.hype_train.progress`
- `channel.hype_train.end`
- `channel.follow`

Notification routing exists for cheers, redemption **add**, Hype Train begin/end, and follows. Redemption update and Hype Train progress are subscribed but currently fall through the unhandled-subscription path. Subscriptions, resubscriptions, gifts, raids, and Watch Streak notices remain on TwitchLib/IRC rather than this EventSub service.

Channel-point add notifications are translated into `ChannelPointRedemptionEvent` and passed through the lock/orchestration service. Allowed redemptions invoke mapped alert handling. Blocked eligible redemptions can send chat feedback and attempt a Helix cancellation/refund.

## Alerts, WebSockets, Media, and OBS

Alert-producing services enqueue `AlertItem` instances into the singleton `AlertService`. If an item contains media, the service first converts its media URL/path to a physical path beneath the configured media root. A missing physical file causes the **entire** alert, including text, to be logged and discarded.

The processor maintains a bounded in-memory FIFO queue:

- At capacity, the oldest queued alert is removed.
- With no overlay client, the current alert is placed back at the front and processing pauses.
- Alerts are eventually dropped after the configured maximum offline duration.
- The server broadcasts one current alert with a generated ID.
- A client must send a matching `ack`; missing acknowledgement requeues the alert until offline expiry.
- After acknowledgement, a matching `done` completes the item; the configured alert timeout prevents a stuck client from blocking the queue forever.

`WebHostWrapper` hosts `/alerts`, `/fullscreen`, static browser assets, `/media` from the configured physical media root, and `/ws` for browser WebSockets. `WebSocketServer` tracks clients, distributes alert payloads, receives `ack`, `done`, and `pong` control messages, and removes stale clients. OBS consumes the same `/alerts` page as an ordinary browser source.

The browser pages explicitly handle MP4, MP3, and GIF paths. Browser/OBS codec support remains external to the application. The `/fullscreen` page exists, but no current production handler emits its `fullscreen` alert type. Detailed path rules, playback behavior, and queue timing are in [Media, Mapping, Overlay, and Windows Environment Setup](MEDIA_ENVIRONMENT_SETUP.md).

## Speech and Generated Text

`TtsService` is the only TTS implementation. Its background runner uses the AWS SDK credential chain and configured Polly region, synthesizes MP3 files under `<media-root>/text_to_speech`, retries an unsupported Neural voice with Standard, and enqueues the resulting `/media` URL. The queue is in memory; skip/reset affect current process state. Shutdown cancels work and removes temporary speech files best-effort.

`AiTextService` is separate from TTS. For qualifying raids, Watch Streaks, resubscription milestones, and gift bundles, it can call the configured OpenAI Responses endpoint for a short line. Missing configuration, HTTP failure, invalid/empty output, or exceptions return no generated line, allowing event-specific speech fallback behavior. OpenAI does not synthesize audio.

See [AWS Polly Setup](AWS_POLLY_SETUP.md) and [OpenAI Setup](OPENAI_SETUP.md) for credentials, costs, and feature-specific fallback details.

## Streamlabs Donations

Streamlabs is optional. When both socket settings are present, `StreamlabsDonationSocketService` starts a Socket.IO connection on a background task, listens for `event` messages of type `donation`, translates each payload into `DonationEvent`, and invokes `DonationAlertService`.

The donation service selects the greatest configured tier not exceeding the amount, then the default media. It records alert history, enqueues a media or text-only browser alert, and requests Polly speech. Missing optional Streamlabs settings disable only this provider. Connection or payload failures are logged inside the provider task and do not establish readiness or fail the controller's already-completed start call. See [Streamlabs Donation Setup](STREAMLABS_SETUP.md).

## JSON Persistence and Ownership

All seven `MediaMapFiles:*` paths pass through `Path.GetFullPath`, so relative values depend on the process working directory rather than the executable/configuration directory. `DataHelperMethods` loads JSON case-insensitively, accepts comments/trailing commas and string enums, returns empty models on load failure through `LoadOrCreateAsync`, and writes through a temporary file followed by move/replace.

| Category | Runtime ownership | Cache/reload behavior | Writes |
| --- | --- | --- | --- |
| Twitch alert media | Operator-managed | Cached after first load | None |
| Excluded users | Operator-managed | Nonempty sets cached; empty data can reload | None |
| First Chatter media | Primarily operator-managed | Loaded once | User-ID backfills and username changes |
| Command mappings | Operator-managed | Populated map cached; empty data can reload | None |
| Watch Streak state | Application-managed | Cached and normalized | Debounced attendance/stream state plus shutdown flush |
| Wheel definitions | UI/application-managed | Repository reloads on queries into current state | Add, rename, delete, item edits, shuffle, visibility/order changes |
| Donation media | Operator-managed | Nonempty/defaulted map cached; entirely empty data can reload | None |

Missing files are not created merely by loading them. Save-capable repositories create parent directories as needed. Alert history, channel-point locks, gift-bundle suppression, connection state, queues, and lookup caches are in-memory only.

## Wheel Spinner

`WheelRepository` persists wheel definitions. `WheelService` owns validation, CRUD operations, hidden-item filtering, effective weights, randomized selection through `IRandomProvider`, concurrent-spin prevention, shuffle, and alert dispatch.

The WinForms layer loads visible items into `WheelSpinnerControl`, performs a 4.5-second weighted-segment animation, and shows the selected `DisplayName` in an in-form overlay panel. The result is not dispatched until the user presses **OK**. Dispatch then uses `AlertType`/`AlertKey` for an existing channel-point or command path, or a generic text-only fallback for blank/unknown metadata and caught exceptions.

The grid manages display name, weight, hidden state, and ordering but does not expose `AlertType` or `AlertKey`. Existing dispatch values are preserved during row updates; new UI-created items have no mapped dispatch until private JSON is edited. See the Wheel schema in the [Media and Environment Guide](MEDIA_ENVIRONMENT_SETUP.md#wheel-definitions).

## Failure Boundaries and Known Limitations

- Configuration failures stop the application before the form; missing media-map files do not.
- EventSub and Streamlabs start asynchronously and lack a unified external-readiness state.
- A controller startup failure is logged rather than propagated to the Start button, and partial-start cleanup can be incomplete.
- Stop Bot attempts state flushes and connection/host shutdown, but Twitch reconnect/watchdog timers remain active until full wrapper disposal. Reusable Stop/Start behavior requires targeted runtime validation and is currently developer-observed as unreliable.
- Twitch chat and several subscription/raid/watch-streak events still depend on TwitchLib/IRC.
- EventSub registers redemption updates and Hype Train progress without notification handlers.
- Core directly references Data, static `AppSettings` is shared mutable process state, and UI infrastructure implements several Core contracts.
- The Testing tab invokes production services and is not an isolated test harness.
- Alert queues, history, caches, channel-point locks, and suppression state are process-local and lost at exit.
- Missing mapped media drops the whole alert rather than degrading to text-only content.
- Browser pages have explicit MP4/MP3/GIF handling only; codecs and autoplay depend on the browser/OBS engine.
- The fullscreen overlay has no current production producer, and overlay CSS uses a solid black background.
- Wheel dispatch metadata is not editable in the grid, and result dispatch waits for user confirmation.
- Relative mapping paths vary with the process working directory.
- There are no automated UI, embedded-host, EventSub provider, or Streamlabs provider tests in the current automated test project.

Future work and historical outcomes are tracked separately in [Feature History](../FEATURE_HISTORY.md); this guide does not treat them as implemented behavior.

## Related Documentation

- [Testing Guide](TESTING.md)
- [Configuration Reference](CONFIGURATION.md)
- [Twitch Developer and OAuth Setup](TWITCH_SETUP.md)
- [Media, Mapping, Overlay, and Windows Environment Setup](MEDIA_ENVIRONMENT_SETUP.md)
- [Streamlabs Donation Setup](STREAMLABS_SETUP.md)
- [OpenAI Alert-Text Setup](OPENAI_SETUP.md)
- [AWS Polly Text-to-Speech Setup](AWS_POLLY_SETUP.md)
- [Feature History](../FEATURE_HISTORY.md)
