# Configuration Reference

This reference describes the configuration behavior implemented by the current TwitchChatBot source. TwitchChatBot is a Windows-only application, and these instructions apply to application version 3.0.0.

## Quick Start

From the repository root, create the required private base file from the tracked template:

```powershell
Copy-Item .\TwitchChatBot\appsettings.example.json .\TwitchChatBot\appsettings.json
```

Edit the new `appsettings.json` with your own Twitch identities, credentials, paths, and integration choices. You may place local overrides, especially secrets, in `TwitchChatBot/appsettings.Development.json`. Both private files are ignored by Git.

The template is structurally valid, but its `YOUR_...` values are placeholders. Passing startup validation does not mean that Twitch, AWS, Streamlabs, OpenAI, media paths, or the overlay are operational.

## Files, Loading, and Precedence

Application configuration is built in `Program.BuildConfiguration()` with `AppContext.BaseDirectory` as its base path. The application loads exactly these providers, in order:

1. `appsettings.json` - required; startup shows an error and exits if it is missing.
2. `appsettings.Development.json` - optional; matching keys override the base file.

Both files use `reloadOnChange: true`. This does not make live editing a supported reconfiguration workflow: some values are captured in constructors, static fields, HTTP clients, or repository paths. Restart the application after changing configuration.

The application configuration builder does **not** add environment-variable or command-line providers. Values such as `Twitch__Channel`, `DOTNET_ENVIRONMENT`, `ASPNETCORE_ENVIRONMENT`, or command-line switches do not override the two application JSON files. The embedded ASP.NET Core host creates its own default host configuration, but the TwitchChatBot settings below still come from the separately built application configuration.

`appsettings.Development.json` is always loaded by that exact name when present. Its use is not conditional on a .NET environment name.

### Build and publish behavior

The UI project applies these copy rules:

| File | Build output | Publish output | Repository policy |
| --- | --- | --- | --- |
| `appsettings.json` | Always copied | Excluded | Ignored; never commit |
| `appsettings.Development.json` | Always copied | Excluded | Ignored; never commit |
| `appsettings.example.json` | Always copied | Always copied | Tracked placeholder template |

The private files can therefore exist beside Debug and Release executables. Treat `bin` and `obj` output as potentially sensitive, and inspect it before sharing. Published output intentionally omits the private files, so an operator must provide `appsettings.json` beside the published executable.

## Status and Validation Terms

- **Required** means startup validation requires a nonblank string, a qualifying integer, or at least one nonblank list item. Failure displays a configuration error and exits before dependency injection or the main form starts.
- **Optional** means the application accepts a missing or blank value.
- **Conditional** means a feature activates only when the stated group is complete.
- **Unused / legacy** means the key exists in the template or an accessor but no production consumer was found. It should not be relied on as functional configuration.
- **Local only** means the value belongs in ignored local configuration, even when it is not itself a credential.

Startup validation checks presence and integer ranges only. It does not authenticate credentials, reject placeholder text, verify URLs, confirm voice names, test directory/file existence, or validate template placeholders.

## Complete Key Inventory

Safe examples below are illustrative. They are not shared credentials and do not guarantee that an external service will accept them.

### Logging

The two logging keys are present in the template, but `Program.ConfigureServices()` adds console and debug logging without binding the `Logging` section. The custom UI text-box logger also enables every level. These keys therefore have no confirmed effect in the current application.

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `Logging:LogLevel:Default` | string, `"Information"` | Unused / ambiguous | No / local only | Conventional .NET log-level key; no binding consumer found. | Not validated. Do not assume it filters current logs. |
| `Logging:LogLevel:TwitchChatBot.UI.Services.EventSubSocketService` | string, `"Error"` | Unused / ambiguous | No / local only | Intended category override; no binding consumer found. | Not validated. Do not assume it filters EventSub logs. |

### Twitch identity

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `Twitch:BotUsername` | string, `"example_bot"` | Required | No / local only | TwitchLib bot login; command and role exclusions. | Must be nonblank. No fallback; an incorrect login prevents useful chat operation. |
| `Twitch:Channel` | string, `"example_channel"` | Required | No / local only | Channel joined by TwitchLib and used for outgoing chat, alerts, wheels, and replay. | Must be nonblank. No existence check; an incorrect name targets or joins the wrong/nonexistent channel. |
| `Twitch:BotUserId` | string, `"123456789"` | Required | No / local only | Numeric bot identity used by test utilities, commands, and moderation calls. | Must be nonblank. Numeric form is not validated; invalid IDs cause Twitch operations to fail. |
| `Twitch:BroadcasterUserId` | string, `"987654321"` | Required | No / local only | Broadcaster identity for EventSub, role lookup, moderation, and redemption updates. | Must be nonblank. Numeric form is not validated; affected Helix/EventSub operations fail. |
| `Twitch:TwitchUrl` | string, `"https://twitch.tv/"` | Required | No / local only | Prefix for command-generated channel URLs. | Must be nonblank. URI syntax and trailing slash are not validated; malformed output URLs may result. |

### Twitch authentication

Client IDs identify an application but are not treated as secrets. Tokens and client secrets are credentials and must never be committed.

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `Auth:BotClientId` | string, `"YOUR_APPLICATION_CLIENT_ID"` | Unused / legacy | No / local only | Accessor exists, but no production consumer was found. | Not startup-validated and no fallback. |
| `Auth:BotClientSecret` | string, `"YOUR_APPLICATION_CLIENT_SECRET"` | Unused / legacy | Yes / never commit | Legacy credential accessor; no production consumer was found. | Not startup-validated and no fallback. |
| `Auth:TwitchClientId` | string, `"YOUR_TWITCH_CLIENT_ID"` | Required | No / local only | `Client-Id` for Twitch Helix, EventSub, lookup, and moderation HTTP clients. | Must be nonblank. Placeholder/validity is not checked; Twitch requests fail if rejected. |
| `Auth:OAuthToken` | string, `"YOUR_OAUTH_TOKEN"` | Required | Yes / never commit | TwitchLib chat credential. | Must be nonblank. Prefix/scope/expiry is not checked; chat connection fails if invalid. |
| `Auth:OAuthBearerBot` | string, `"YOUR_OAUTH_BEARER_TOKEN"` | Required | Yes / never commit | Bearer token for the bot Helix client and bot-authorized moderation operations. | Must be nonblank. Scope/expiry is not checked; bot Helix operations fail if invalid. |
| `Auth:AccessToken` | string, `"YOUR_USER_ACCESS_TOKEN"` | Required | Yes / never commit | User bearer token for Helix, EventSub subscriptions, role lookup, and broadcaster-authorized moderation/redemption operations. | Must be nonblank. Scope/expiry is not checked; dependent Twitch operations fail if invalid. |
| `Auth:AppAccessToken` | string, `"YOUR_APP_ACCESS_TOKEN"` | Unused / legacy | Yes / never commit | Accessor exists, but no production consumer was found. | Not startup-validated and no fallback. |
| `Auth:RefreshToken` | string, `"YOUR_REFRESH_TOKEN"` | Unused / legacy | Yes / never commit | Accessor exists, but no refresh flow was found. | Not startup-validated; the application does not refresh tokens from this key. |

### Ad reminder and fallback identity

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `Ads:AdInitialSeconds` | integer, `120` | Required, minimum 0 | No / local only | Initial delay before the internal `!ads` reminder timer fires. | Must parse as an integer and be at least 0; otherwise startup exits. |
| `Ads:AdIntervalMinutes` | integer, `60` | Required, minimum 1 | No / local only | Recurring ad-reminder interval. | Must parse as an integer and be at least 1; otherwise startup exits. |
| `Ads:DefaultUserName` | string, `"anonymous"` | Required | No / local only | Fallback username used when event/replay data lacks one. | Must be nonblank. No runtime fallback beyond this configured value. |

### Embedded web host and overlay

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `WebHost:BaseUrl` | string URI, `"http://localhost:3000/"` | Required | No / local only | Kestrel listen URL used by the browser/OBS overlay host. | Only nonblank validation. Invalid syntax, an unavailable port, or insufficient URL permissions makes web-host startup fail when the bot starts. |
| `WebHost:WebRoot` | string path, `"wwwroot"` | Required | No / local only | Static web root and source of `alerts.html` and `fullscreen.html`. | Only nonblank validation. Missing/wrong paths prevent overlay files from being served. Relative-path resolution depends on host/process context; retain the copied `wwwroot` layout. |

### EventSub WebSocket

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `EventSub:Secret` | string, `"YOUR_EVENTSUB_SECRET"` | Unused / legacy | Yes / never commit | Webhook-era secret accessor; the current transport is WebSocket and no consumer was found. | Not startup-validated and has no effect on current EventSub handling. |
| `EventSub:CallbackUrl` | string URI, `"https://example.invalid/eventsub"` | Unused / legacy | No / local only | Webhook-era callback accessor; no consumer was found. | Not startup-validated and has no effect on current EventSub handling. |
| `EventSub:Uri` | string URI, `"wss://eventsub.wss.twitch.tv/ws"` | Required | No / local only | Primary EventSub WebSocket endpoint; Twitch-provided reconnect URLs temporarily supersede it. | Only nonblank validation. `new Uri(...)` or connection fails for malformed/unreachable values; reconnect logic retries. |
| `EventSub:PostSubscriptionsUrl` | string URI, `"https://api.twitch.tv/helix/eventsub/subscriptions"` | Required | No / local only | Helix endpoint used to create WebSocket subscriptions. | Only nonblank validation. Bad URLs or rejected requests leave event types unsubscribed and are logged. |
| `EventSub:Validate` | string URI, `"https://id.twitch.tv/oauth2/validate"` | Required | No / local only | Validates the user token to resolve its user ID for follow subscriptions. | Only nonblank validation. Failure can cause `channel.follow` subscription to be skipped while other subscriptions continue. |

### Runtime JSON paths

These values select JSON data files used by repositories. The path strings are startup-required, but the files are not checked during startup. `Path.GetFullPath` resolves relative values against the process working directory, not the application configuration base path. Absolute paths are safest for these seven JSON settings. Media values stored inside the JSON should instead be relative to `MediaBase:TwitchAlertsFolder`; ordinary absolute media values do not map correctly to browser `/media` URLs. See [Media, Mapping, Overlay, and Windows Environment Setup](MEDIA_ENVIRONMENT_SETUP.md).

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `MediaMapFiles:TwitchAlertMedia` | string path, `"C:\\TwitchChatBotData\\twitchAlertsMediaMap.json"` | Required | No / local only | Alert media map repository. | Must be nonblank. Missing/malformed data is logged and replaced in memory with an empty map; alerts may have no playable media. |
| `MediaMapFiles:ExcludedUsersMedia` | string path, `"C:\\TwitchChatBotData\\excludedUsers.json"` | Required | No / local only | Excluded-user set. | Must be nonblank. Missing/malformed data becomes an empty set, so no users are excluded. |
| `MediaMapFiles:FirstChattersMedia` | string path, `"C:\\TwitchChatBotData\\firstChatMediaMap_v2.json"` | Required | No / local only | First-chatter media map and migration target. | Must be nonblank. Missing/malformed data becomes an empty map; legacy-format migration is described below. |
| `MediaMapFiles:CommandAlertMedia` | string path, `"C:\\TwitchChatBotData\\commandsMediaMap.json"` | Required | No / local only | Chat-command text/media definitions. | Must be nonblank. Missing/malformed data becomes an empty map, leaving no configured media commands. |
| `MediaMapFiles:UserWatchStreakMedia` | string path, `"C:\\TwitchChatBotData\\watch_streaks_v2.json"` | Required | No / local only | Mutable watch-streak state. | Must be nonblank. Missing/malformed data becomes a new empty state; later saves create directories/files. |
| `MediaMapFiles:WheelMedia` | string path, `"C:\\TwitchChatBotData\\wheelsMediaMap.json"` | Required | No / local only | Mutable wheel definitions and items. | Must be nonblank. Missing/malformed data becomes an empty wheel list; UI saves create directories/files. |
| `MediaMapFiles:DonationMedia` | string path, `"C:\\TwitchChatBotData\\donationMediaMap.json"` | Required | No / local only | Streamlabs donation tier/default media map. | Must be nonblank even when Streamlabs is disabled. Missing/malformed data becomes an empty donation map. |

### Media base

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `MediaBase:TwitchAlertsFolder` | string directory, `"C:\\TwitchChatBotMedia"` | Required | No / local only | Physical `/media` root, alert path resolution, and `text_to_speech` output directory. | Only nonblank validation. The TTS service creates its subdirectory, but the embedded host requires a valid physical root; missing, inaccessible, or mismatched paths break media serving and can prevent host/TTS startup. |

### Twitch Helix role endpoints

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `Chatters:ModsUrl` | string URI template, `"https://api.twitch.tv/helix/moderation/moderators?broadcaster_id={broadcasterId}"` | Required | No / local only | Moderator lookup endpoint. | Must be nonblank. Placeholder and URI syntax are not validated; bad values prevent moderator resolution. |
| `Chatters:VipUrl` | string URI template, `"https://api.twitch.tv/helix/channels/vips?broadcaster_id={broadcasterId}"` | Required | No / local only | VIP lookup endpoint. | Must be nonblank. Placeholder and URI syntax are not validated; bad values prevent VIP resolution. |
| `Chatters:InitialDelay` | JSON number, `60` | Unused / legacy | No / local only | Accessor exists as a string, but no production consumer was found. | Not startup-validated and has no confirmed effect. |
| `Chatters:ContinuousDelay` | JSON number, `300` | Unused / legacy | No / local only | Accessor exists as a string, but no production consumer was found. | Not startup-validated and has no confirmed effect. |

### Text-to-speech and AWS Polly

See [AWS Polly Text-to-Speech Setup](AWS_POLLY_SETUP.md) for the SDK v4 credential chain, least-privilege IAM permission, voice and region selection, runtime verification, and billing precautions.

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `TTS:Engine` | string, `"Polly"` | Unused / legacy | No / local only | Template suggests engine selection, but no production consumer was found; Polly is hard-wired. | Not startup-validated. Changing it does not select another engine. |
| `TTS:PollyRegion` | string, `"us-east-1"` | Required | No / local only | AWS region passed to `RegionEndpoint.GetBySystemName`. | Must be nonblank; semantic validity is not checked. A defensive `us-east-1` fallback exists in `TtsService`, but normal startup validation prevents blank input from reaching it. |
| `TTS:DefaultSpeaker` | string, `"Matthew"` | Required | No / local only | Default Polly voice when an event-specific voice is not used. | Must be nonblank. A defensive `Matthew` fallback exists after startup validation; unsupported voices can make Polly synthesis fail. |
| `TTS:MaxChars` | integer, `350` | Required, minimum 1 | No / local only | Maximum sanitized TTS text length. | Must be an integer at least 1. A defensive 350 fallback exists for nonpositive values, but startup rejects them first. |
| `TTS:AllowedVoices` | string array, `["Matthew", "Joanna"]` | Required, nonempty | No / local only | Case-insensitive allowlist for requested voice overrides. A comma-separated scalar is also accepted by the accessor. | At least one nonblank item/value is required. A defensive `Matthew` fallback exists for an empty resulting set, but startup normally rejects one. |
| `TTS:Voices:Cheer` | string, `"Matthew"` | Required | No / local only | Polly voice for cheer speech. | Must be nonblank; voice support is not validated. Polly may reject an unsupported voice. |
| `TTS:Voices:Subscribe` | string, `"Matthew"` | Required | No / local only | Polly voice for new subscriptions. | Same validation and failure behavior as other event voices. |
| `TTS:Voices:SubscriptionMessage` | string, `"Matthew"` | Required | No / local only | Polly voice for resubscription messages. | Same validation and failure behavior as other event voices. |
| `TTS:Voices:SingleGiftSub` | string, `"Matthew"` | Required | No / local only | Polly voice for individual gifted subscriptions. | Same validation and failure behavior as other event voices. |
| `TTS:Voices:GiftSubs` | string, `"Matthew"` | Required | No / local only | Polly voice for gift-sub bundles. | Same validation and failure behavior as other event voices. |
| `TTS:Voices:Raid` | string, `"Matthew"` | Required | No / local only | Polly voice for raids. | Same validation and failure behavior as other event voices. |
| `TTS:Voices:Follow` | string, `"Matthew"` | Required | No / local only | Polly voice for follows and, currently, donation speech. | Same validation and failure behavior as other event voices. |
| `TTS:Voices:WatchStreak` | string, `"Matthew"` | Required | No / local only | Polly voice for watch-streak alerts. | Same validation and failure behavior as other event voices. |
| `TTS:Voices:ChannelPoints` | string, `"Matthew"` | Required | No / local only | Polly voice for channel-point speech. | Same validation and failure behavior as other event voices. |

AWS credentials are not application JSON keys. `TtsService` delegates credential discovery to the AWS SDK's `DefaultAWSCredentialsIdentityResolver`, then constructs `AmazonPollyClient` with the configured region. The AWS SDK for .NET v4 default chain supports complete session or basic credential environment variables, web-identity environment variables, configured profiles including `AWS_PROFILE` and `[default]`, container credentials, and EC2 instance metadata in its documented order. TwitchChatBot supplies no explicit credentials or profile selection. If credentials cannot be resolved, TTS requests are skipped and resolution is retried periodically; the rest of the application can continue.

### TTS templates

All template values are startup-required nonblank strings. There is no general template parser validation. Unknown or missing placeholders remain a content-quality issue; supported replacements are applied by the alert handler.

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `TTS:Templates:Follow` | string, `"{username} followed the channel."` | Required | No / local only | Follow speech; replaces `{username}`. | Nonblank only. |
| `TTS:Templates:Raid` | string, `"{raider} raided with {viewers} viewers."` | Required | No / local only | Raid speech; replaces `{raider}` and `{viewers}`. | Nonblank only. |
| `TTS:Templates:SingleGiftSub` | string, `"{username} gifted Tier {tier} to {recipient}."` | Required | No / local only | Single-gift speech; replaces `{username}`, `{tier}`, and `{recipient}`. | Nonblank only. |
| `TTS:Templates:MysteryGift` | string, `"{username} gifted {numOfSubs} Tier {tier} subscriptions."` | Required | No / local only | Gift-bundle speech; replaces `{username}`, `{numOfSubs}`, and `{tier}`. | Nonblank only. |
| `TTS:Templates:CheerNoMessage` | string, `"{username} cheered {bits} bits."` | Required | No / local only | Cheer speech when no chat message is present; replaces `{username}` and `{bits}`. | Nonblank only. |
| `TTS:Templates:SubNoMessage` | string, `"{username} subscribed at Tier {tier}."` | Required | No / local only | New-subscription speech; replaces `{username}` and `{tier}`. | Nonblank only. |
| `TTS:Templates:ReSub` | string, `"{username} resubscribed for {months} months at Tier {tier}."` | Required | No / local only | Resubscription fallback speech; replaces `{username}`, `{months}`, and `{tier}`. | Nonblank only. |
| `TTS:Templates:WatchStreak` | string, `"{username} has a {streak}-stream streak."` | Required | No / local only | Watch-streak fallback speech; current source replaces `{username}` and `{streak}`. | Nonblank only. The tracked example currently uses `{streakCount}`, which is not replaced by the current handler; use `{streak}` in local configuration. |

### Twitch Helix command endpoints

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `Commands:UsersByLogin` | string URI template, `"https://api.twitch.tv/helix/users?login={login}"` | Required | No / local only | User/profile lookup by login. | Nonblank only. Missing `{login}` or bad URI produces incorrect/failing lookups. |
| `Commands:StreamsByUserId` | string URI template, `"https://api.twitch.tv/helix/streams?user_id={id}"` | Required | No / local only | Live-stream lookup. | Nonblank only. Missing `{id}` or bad URI produces incorrect/failing lookups. |
| `Commands:ChannelInfoByBroadcasterId` | string URI template, `"https://api.twitch.tv/helix/channels?broadcaster_id={id}"` | Required | No / local only | Channel metadata lookup. | Nonblank only. Missing `{id}` or bad URI produces incorrect/failing lookups. |
| `Commands:VideosByUserId` | string URI template, `"https://api.twitch.tv/helix/videos?user_id={id}&type=archive&first=1"` | Required | No / local only | Latest archived-video lookup. | Nonblank only. Missing `{id}` or bad URI produces incorrect/failing lookups. |
| `Commands:GamesByGameId` | string URI template, `"https://api.twitch.tv/helix/games?id={gameid}"` | Required | No / local only | Game-name lookup. | Nonblank only. Missing `{gameid}` or bad URI produces incorrect/failing lookups. |

### Moderation

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `Moderation:ClearSpecialAlertUsers` | string array, `["trusted_mod"]` | Optional | No / local only | Case-insensitive users allowed to run special clear-alert behavior in addition to the broadcaster. A comma-separated scalar is also accepted. | Missing/blank becomes an empty set. Usernames are trimmed; blanks are removed. |

### Streamlabs

Streamlabs is enabled only when both settings are nonblank. There is no separate enabled flag.

See [Streamlabs Donation Setup](STREAMLABS_SETUP.md) for the external socket-token flow, donation event processing, media selection, connectivity verification, Polly dependency, and provider limitations.

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `Streamlabs:SocketUrl` | string URI, `"https://streamlabs.example.invalid"` | Conditional | No / local only | Socket.IO provider endpoint. | Not startup-validated. Missing/blank with either setting disables Streamlabs; malformed/unreachable values fail the provider without stopping core application startup. |
| `Streamlabs:SocketToken` | string, `""` or `"YOUR_STREAMLABS_TOKEN"` | Conditional | Yes / never commit | Socket.IO authentication query value. | Not startup-validated. Missing/blank with either setting disables Streamlabs; invalid credentials prevent provider operation. |

### OpenAI alert text

OpenAI-generated text is optional because the API key may be blank. Supporting OpenAI behavior settings are nevertheless startup-required by the current validator.

See [OpenAI Alert-Text Setup](OPENAI_SETUP.md) for API-key and billing setup, the data included in prompts, event-specific speech fallback behavior, safe verification, and troubleshooting.

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `OpenAI:BaseUrl` | string URI, `"https://api.openai.com/v1/"` | Required | No / local only | Base address for the named OpenAI HTTP client. | Nonblank only. `new Uri(...)` runs during service setup; malformed values can terminate startup after validation. Retain a trailing slash so relative `responses` requests resolve as intended. |
| `OpenAI:ApiKey` | string, `""` or `"YOUR_OPENAI_API_KEY"` | Optional | Yes / never commit | Bearer credential for optional AI alert generation. | Missing/blank becomes an empty string. Requests then fail and AI generation returns no line. The visual alert continues; speech fallback depends on the event path. |
| `OpenAI:Model` | string, `"gpt-4o-mini"` | Required | No / local only | Model value sent to the configured `responses` endpoint. | Nonblank only. Unsupported values cause generation requests to return no generated line. |
| `OpenAI:DefaultAlertTone` | string, `"hype"` | Required | No / local only | Default tone stored in alert contexts. | Nonblank only. The current prompt builders do not read the `Tone` property, so changing it has no confirmed prompt effect. |
| `OpenAI:DefaultAlertMaxWords` | integer, `14` | Required, minimum 1 | No / local only | Maximum generated words requested and enforced by local truncation. | Must be an integer at least 1; otherwise startup exits. |
| `OpenAI:WatchStreakThreshold` | integer, `20` | Required, minimum 0 | No / local only | Minimum watch streak at which AI text is attempted. | Must be an integer at least 0; otherwise startup exits. At or above the threshold, unsuccessful generation leaves the visual alert intact but has no speech fallback. |
| `OpenAI:GiftSubThreshold` | integer, `5` | Required, minimum 0 | No / local only | Minimum gift count at which AI text is attempted. | Must be an integer at least 0; otherwise startup exits. At or above the threshold, unsuccessful generation leaves the visual alert intact but has no speech fallback. |

### Alert queue behavior

| Key | Type / safe example | Status | Secret / commit | Purpose and consumer | Validation, fallback, and failure behavior |
| --- | --- | --- | --- | --- | --- |
| `AlertSettings:AlertTimeOut` | integer seconds, `60` | Required, minimum 1 | No / local only | Maximum wait for overlay `DONE` after acknowledgement. | Invalid or below 1 stops startup. On timeout, the alert is forcibly completed. |
| `AlertSettings:AcknowledgeTimeOut` | integer seconds, `10` | Required, minimum 1 | No / local only | Maximum wait for overlay acknowledgement. | Invalid or below 1 stops startup. Unacknowledged alerts are held/requeued until offline expiry. |
| `AlertSettings:MaxQueueSize` | integer count, `50` | Required, minimum 1 | No / local only | Maximum queued alerts. | Invalid or below 1 stops startup. At capacity, the oldest queued alert is dropped. |
| `AlertSettings:MaxOfflineQueueTime` | integer seconds, `60` | Required, minimum 1 | No / local only | Maximum time alerts are retained while the overlay is offline or not acknowledging. | Invalid or below 1 stops startup. Expired alerts are dropped. |
| `AlertSettings:RetryTaskDelay` | integer milliseconds, `500` | Required, minimum 1 | No / local only | Delay before retrying an unacknowledged/failed alert. | Invalid or below 1 stops startup. |

## Representative Safe JSON

This fragment demonstrates JSON shape and override behavior. It is intentionally incomplete and cannot replace the full tracked template.

```json
{
  "Twitch": {
    "BotUsername": "example_bot",
    "Channel": "example_channel",
    "BotUserId": "123456789",
    "BroadcasterUserId": "987654321",
    "TwitchUrl": "https://twitch.tv/"
  },
  "Auth": {
    "TwitchClientId": "YOUR_TWITCH_CLIENT_ID",
    "OAuthToken": "YOUR_OAUTH_TOKEN",
    "OAuthBearerBot": "YOUR_OAUTH_BEARER_TOKEN",
    "AccessToken": "YOUR_USER_ACCESS_TOKEN"
  },
  "MediaMapFiles": {
    "TwitchAlertMedia": "C:\\TwitchChatBotData\\twitchAlertsMediaMap.json"
  },
  "Streamlabs": {
    "SocketUrl": "",
    "SocketToken": ""
  },
  "OpenAI": {
    "ApiKey": ""
  }
}
```

For a real base file, copy `appsettings.example.json` and preserve all startup-required keys. A minimal local override can contain only the keys being replaced:

```json
{
  "Auth": {
    "OAuthToken": "YOUR_PRIVATE_LOCAL_VALUE",
    "OAuthBearerBot": "YOUR_PRIVATE_LOCAL_VALUE",
    "AccessToken": "YOUR_PRIVATE_LOCAL_VALUE"
  },
  "OpenAI": {
    "ApiKey": "YOUR_PRIVATE_LOCAL_VALUE"
  }
}
```

## Persisted Runtime JSON

The files selected by `MediaMapFiles:*` are application data, not additional configuration providers. Their contents do not override `appsettings.json`; the startup files only tell repositories where the data lives.

All repositories deserialize property names case-insensitively, permit JSON comments and trailing commas, and accept string enum names. General `LoadOrCreateAsync` behavior logs missing, empty, malformed, inaccessible, or incompatible data and returns the repository's empty model. Saves create parent directories and replace the target through a temporary file.

| Path setting | Data and lifecycle | Migration/normalization behavior |
| --- | --- | --- |
| `MediaMapFiles:TwitchAlertMedia` | Cached alert media definitions for channel points, cheers, follows, Hype Train, raids, subscriptions, gifts, and watch streaks. Read-only in the repository. | No explicit migration. Missing/invalid content becomes an empty model. |
| `MediaMapFiles:ExcludedUsersMedia` | Cached case-insensitive set used to suppress selected users. Read-only in the repository. | No explicit migration. Missing/invalid content becomes an empty set. |
| `MediaMapFiles:FirstChattersMedia` | First-chatter items containing user ID, current username, media, and allowed days. The repository may save identity updates. | Contains a fallback intended to convert the legacy `{ "username": "media-path" }` dictionary and save item records. However, the new-format model initializes its item list, so a legacy object can be accepted as an empty new-format model before that fallback is reached. Automatic legacy conversion is therefore not guaranteed. Loaded item records do backfill user IDs/usernames and normalize null day lists. |
| `MediaMapFiles:CommandAlertMedia` | Cached command items containing command, optional text, and optional media. Read-only in the repository. | No explicit migration. Missing/invalid content becomes an empty command list. |
| `MediaMapFiles:UserWatchStreakMedia` | Mutable versioned stream index and per-user streak statistics. Writes are debounced by the service and flushed during shutdown. | Normalizes null/negative legacy fields, prefers user-ID dictionary keys, can migrate username-keyed entries when identities become known, and merges duplicate identities. The stored version remains 1 in current code. |
| `MediaMapFiles:WheelMedia` | Mutable list of wheels and weighted/visible items, managed by the Wheel UI and service. | No explicit migration. Missing/invalid content becomes an empty list. |
| `MediaMapFiles:DonationMedia` | Cached default and tiered Streamlabs donation media. Read-only in the repository. | No explicit migration. Missing/invalid content becomes an empty default and tier list. |

Alert history is in memory only. Polly MP3 files are temporary runtime media under `<MediaBase:TwitchAlertsFolder>\text_to_speech` and are deleted best-effort when `TtsService` is disposed.

## Known Unused, Legacy, and Ambiguous Keys

Do not treat these keys as active feature controls:

- Logging filters: `Logging:LogLevel:Default` and `Logging:LogLevel:TwitchChatBot.UI.Services.EventSubSocketService` are not bound to logging.
- Unused authentication accessors: `Auth:BotClientId`, `Auth:BotClientSecret`, `Auth:AppAccessToken`, and `Auth:RefreshToken`.
- Webhook-era EventSub accessors: `EventSub:Secret` and `EventSub:CallbackUrl`.
- Unused chatter timing accessors: `Chatters:InitialDelay` and `Chatters:ContinuousDelay`.
- Unused engine selector: `TTS:Engine`; Polly remains the implemented engine.
- `UiMedia:SpinButton` has an accessor in `AppSettings` but is absent from the tracked template and has no production consumer. It is not a supported active setting.
- `OpenAI:DefaultAlertTone` is required and placed on alert contexts, but the current AI prompt builders do not consume the tone value.
- First Chatter legacy-dictionary migration code exists, but current new-format deserialization can return an empty initialized model before the legacy fallback is attempted. Back up legacy data and verify conversion rather than assuming it will migrate automatically.

## Troubleshooting

### The application exits before showing the main window

- Confirm `appsettings.json` is beside the executable, not only in the source directory.
- Check both JSON files for syntax errors. The later Development file can invalidate otherwise valid base configuration.
- Read the displayed list of missing or invalid keys. Error messages identify keys but intentionally omit their values.
- Remember that placeholder strings pass presence checks; authentication can still fail later.

### Twitch chat or Helix features fail

- Verify the bot/channel names and numeric IDs belong to the intended accounts.
- Confirm each token has the appropriate account, scopes, format, and validity for its consumer.
- Check the URI templates still contain their documented replacement tokens.
- Token refresh is not implemented through `Auth:RefreshToken`; replace expired tokens manually.

### Browser or OBS overlays fail

- Confirm `WebHost:BaseUrl` is a valid available local URL and port.
- Confirm the copied `wwwroot` contains `alerts.html` and `fullscreen.html`.
- Confirm `MediaBase:TwitchAlertsFolder` exists and matches paths referenced by media-map data.
- Use `/alerts` or `/fullscreen` at the configured base URL; WebSocket traffic uses `/ws`.

### Polly speech is skipped

- Confirm AWS credentials are discoverable by the AWS SDK default resolver.
- Confirm `TTS:PollyRegion`, the selected voice, and AWS permissions support Polly synthesis.
- Confirm the media base is writable so `text_to_speech` can be created and served.

### Optional providers do not run

- Streamlabs requires both `Streamlabs:SocketUrl` and `Streamlabs:SocketToken`; either blank disables it.
- OpenAI accepts a blank API key, but qualifying paths still attempt generation and receive no generated line. Raid speech uses its template fallback; qualifying watch streaks, gift bundles, and annual resubscription milestones retain their visual alerts but produce no fallback speech.

## Security Guidance

- Never commit `appsettings.json`, `appsettings.Development.json`, OAuth tokens, bearer/access/refresh tokens, client secrets, Streamlabs tokens, OpenAI keys, or AWS credentials.
- Keep placeholders in `appsettings.example.json`; do not turn it into a working personal configuration.
- Prefer ignored local overrides for secrets, restrict file access to the developer account, and rotate credentials after accidental disclosure.
- Do not paste secrets into issues, logs, screenshots, documentation, tests, or chat transcripts.
- Inspect copied Debug/Release output before sharing it. Private configuration is copied beside local build executables.
- Persisted media maps can contain usernames, local paths, or operational details. Review them before sharing even when they contain no authentication secret.

Sanitized schemas and copyable examples for all seven files are maintained in the [media and environment guide](MEDIA_ENVIRONMENT_SETUP.md#template-index). The tracked examples are documentation artifacts and are not automatically loaded or copied into private runtime data.

## Source of Truth

This document reflects `Program.cs`, `AppSettings.cs`, the tracked `appsettings.example.json`, the UI project copy rules, configuration tests, current consumers, and JSON repositories. If documentation and executable behavior differ, the current repository implementation is authoritative.
