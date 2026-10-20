# Twitch Developer and OAuth Setup

This guide explains how to connect TwitchChatBot 3.0.0 to Twitch using the integration behavior implemented in this repository. It covers account ownership, developer application registration, OAuth scopes and tokens, configuration, EventSub, Helix, first-run verification, and troubleshooting.

For every application setting, including validation and fallback behavior, see the [Configuration Reference](CONFIGURATION.md). Keep Twitch's official documentation open while provisioning credentials because Twitch may change its developer-console and authorization procedures.

## Integration Overview

TwitchChatBot currently divides Twitch work across three interfaces:

- **TwitchLib and IRC** receive and send chat messages. IRC messages drive commands and First Chatter behavior. IRC notices and membership events also drive subscription, resubscription, gift-subscription, raid, and watch-streak behavior.
- **EventSub WebSocket** receives follows, cheers, channel-point redemptions, and Hype Train events.
- **Twitch Helix** performs user and profile-image lookups, stream/channel/video/game lookups, moderator and VIP lookups, moderation actions, and channel-point redemption status changes.

The planned migration from Twitch IRC to Twitch's newer Chat API and chat EventSub model has **not** been implemented. Do not use future Chat API scopes such as `user:read:chat`, `user:write:chat`, `user:bot`, or `channel:bot` as substitutes for the current IRC scopes.

## Twitch Accounts and Identities

### Recommended two-account setup

Use two Twitch accounts:

1. The **broadcaster account** owns the channel. Its login is `Twitch:Channel`, and its numeric ID is `Twitch:BroadcasterUserId`. This account authorizes the user access token used for broadcaster Helix calls and EventSub.
2. The **bot account** connects to IRC. Its login is `Twitch:BotUsername`, and its numeric ID is `Twitch:BotUserId`. This account authorizes the IRC and bot Helix credentials. Give this account moderator status in the broadcaster's channel if it will perform timeouts or Nuke moderation.

Separate accounts are recommended because they make token ownership and moderation authority clear and limit which permissions each identity receives.

### Single-account configuration

A single Twitch account can technically fill both roles if the bot username, channel name, both numeric IDs, and all token ownership are aligned. The code does not prohibit this arrangement, but its services model the bot and broadcaster as separate roles. The two-account setup is the supported recommendation unless the developer has deliberately reviewed the single-account tradeoffs.

### Obtaining numeric user IDs

TwitchChatBot requires numeric IDs, not display names. Use an official Twitch API method:

- A validated user access token response includes the authorizing account's `user_id` and `login`.
- Twitch's **Get Users** Helix endpoint can resolve a login to an ID. With the official Twitch CLI configured, for example:

  ```powershell
  twitch api get users -q login=example_bot
  twitch api get users -q login=example_channel
  ```

Confirm that the returned bot ID belongs to `Twitch:BotUsername` and that the returned broadcaster ID belongs to `Twitch:Channel`. The application checks only that these settings are nonblank; it does not verify that an ID matches a login.

## Register a Twitch Developer Application

1. Sign in to the [Twitch Developer Console](https://dev.twitch.tv/console) with the account that will own the application.
2. Verify the account and enable two-factor authentication as required by Twitch.
3. Open **Applications**, select **Register Your Application**, and enter a unique application name.
4. Choose an appropriate application category.
5. Add an OAuth redirect URL. If you use the official Twitch CLI procedure below, register `http://localhost:3000` exactly as required by the CLI documentation.
6. Create the application, open **Manage**, and copy its Client ID into private local configuration as `Auth:TwitchClientId`.

The Client ID identifies the developer application and is sent with Helix and EventSub requests. Tokens used by TwitchChatBot must be issued for that same Client ID.

### Client secrets and redirect URLs

A client secret is password-equivalent. The official Twitch CLI uses the Client ID and client secret to obtain user tokens, but TwitchChatBot does not read a client secret during normal operation. Store the secret only in the external OAuth tool's protected configuration and never place it in tracked files, documentation, logs, screenshots, or issue reports.

TwitchChatBot does not implement an OAuth callback, authorization page, token exchange, or device flow. The redirect URL belongs to the external OAuth flow used to generate tokens. It is unrelated to `EventSub:CallbackUrl`, which is an unused webhook-era setting; current EventSub delivery uses WebSockets and needs neither a callback URL nor an EventSub secret.

See Twitch's [application-registration instructions](https://dev.twitch.tv/docs/authentication/register-app) for the current console requirements.

## Generate User Access Tokens

Use an official OAuth flow or the official Twitch CLI. Do not use websites that ask you to paste a client secret, access token, refresh token, or Twitch password.

The procedure below uses the [official Twitch CLI](https://dev.twitch.tv/docs/cli/), which is an external prerequisite and is not included in this repository.

### Configure the official Twitch CLI

1. Install the Twitch CLI using Twitch's current Windows instructions.
2. Run the interactive configuration command:

   ```powershell
   twitch configure
   ```

3. Enter the developer application's Client ID and client secret only at the CLI's prompts. Avoid command-line flags containing the secret because shell history and process inspection can expose it.

The CLI user-token flow opens a browser. Before authorizing, sign in as the account that must own that token. If the wrong Twitch account is already signed in, sign out or use a separate browser profile.

### Bot token

Generate a bot user token with the two current IRC scopes:

```powershell
twitch token --user-token --scopes "chat:read chat:edit"
```

If the bot will execute timeout/Nuke operations, include the conditional moderation scope:

```powershell
twitch token --user-token --scopes "chat:read chat:edit moderator:manage:banned_users"
```

Authorize this request while signed in as `Twitch:BotUsername`. The returned raw user access token can supply both bot settings:

- Store `oauth:<raw-token>` as `Auth:OAuthToken` for IRC.
- Store `<raw-token>` as `Auth:OAuthBearerBot` for bot-authorized Helix calls.

You may instead issue separate bot tokens, but each must belong to the bot account, use the same configured Client ID, and contain the scopes needed by its consumer.

### Broadcaster token

For the full currently configured EventSub and broadcaster Helix feature set, request only the scopes for the features you intend to use. A complete set is:

```text
bits:read
channel:manage:redemptions
channel:read:hype_train
moderator:read:followers
moderation:read
channel:read:vips
moderator:manage:banned_users
```

An official CLI request for that complete set is:

```powershell
twitch token --user-token --scopes "bits:read channel:manage:redemptions channel:read:hype_train moderator:read:followers moderation:read channel:read:vips moderator:manage:banned_users"
```

Authorize this request while signed in as `Twitch:Channel`, then store the returned raw token as `Auth:AccessToken`. Omit feature-dependent scopes only after reviewing the [scope matrix](#oauth-scope-matrix); the application still attempts all seven EventSub subscriptions and logs failures for subscriptions it cannot create.

### Validate tokens before use

Validate each raw token against Twitch's official endpoint. Do not add `oauth:` or `Bearer ` to a validation request:

```powershell
$headers = @{ Authorization = "OAuth YOUR_RAW_TOKEN" }
Invoke-RestMethod -Uri "https://id.twitch.tv/oauth2/validate" -Headers $headers
```

Use a placeholder while preparing commands and enter the real token only in your private terminal session. Do not save token-bearing commands in scripts or screenshots. Confirm the returned `client_id`, `login`, `user_id`, `scopes`, and `expires_in` against the intended application and account.

The application validates only the broadcaster token while setting up EventSub, and it uses that result primarily to obtain the user ID for follow subscriptions. It does not comprehensively validate all identities, scopes, client IDs, or expiry values.

### Expiration and renewal

Access tokens expire or may be revoked. TwitchChatBot does not automatically refresh them, even though an unused `Auth:RefreshToken` accessor exists. When a token expires:

1. Use the external official OAuth flow to renew or generate the token.
2. Validate its owner, Client ID, scopes, and expiry.
3. Replace the corresponding value in ignored local configuration.
4. Restart TwitchChatBot and repeat the first-run checks.

Authorization Code Grant tokens can have refresh tokens, but this repository does not consume them. See Twitch's [token renewal documentation](https://dev.twitch.tv/docs/authentication/refresh-tokens/) before designing any external renewal process.

## Exact Twitch Configuration Keys

The source-backed JSON paths are shown below. The Twitch-prefixed C# accessor names do not add `Twitch` to the JSON key path.

| Story concept | Exact JSON path | Owner and purpose | Format |
| --- | --- | --- | --- |
| Twitch client ID | `Auth:TwitchClientId` | Developer application; used by Helix and EventSub | Raw Client ID |
| Twitch OAuth token | `Auth:OAuthToken` | Bot account; TwitchLib IRC password | `oauth:<raw-token>` |
| Twitch bot bearer token | `Auth:OAuthBearerBot` | Bot account; bot-authorized Helix moderation | Raw user access token |
| Twitch access token | `Auth:AccessToken` | Broadcaster account; general Helix and EventSub | Raw user access token |
| Bot username | `Twitch:BotUsername` | Login of the bot token's account | Twitch login, not display name |
| Bot user ID | `Twitch:BotUserId` | Numeric ID of the bot account | Numeric ID stored as a string |
| Channel | `Twitch:Channel` | Broadcaster login/channel joined by IRC | Twitch login, not URL |
| Broadcaster user ID | `Twitch:BroadcasterUserId` | Numeric ID of the broadcaster/channel | Numeric ID stored as a string |

`Auth:TwitchOAuthToken`, `Auth:TwitchOAuthBearerBot`, and `Auth:TwitchAccessToken` are **not** implemented JSON paths. Use the exact paths in the table and in `appsettings.example.json`.

The application constructs Helix headers as `Authorization: Bearer <configured-token>`. Do not include `Bearer ` or `oauth:` in either bearer-token setting. In contrast, the IRC credential is passed directly to TwitchLib and must include the `oauth:` prefix.

### File placement and precedence

From the repository root, copy the tracked template:

```powershell
Copy-Item .\TwitchChatBot\appsettings.example.json .\TwitchChatBot\appsettings.json
```

At runtime, configuration is loaded relative to the executable in this order:

1. Required `appsettings.json`.
2. Optional `appsettings.Development.json`, whose matching values override the base file.

Both private files are copied beside local build executables. Never commit either file or share build output without checking it for copied secrets. Commit only the placeholder-based `appsettings.example.json`. Environment variables and command-line arguments do not override these application settings.

See the [Configuration Reference](CONFIGURATION.md) for the complete inventory, startup validation, copy/publish behavior, and safe JSON structure.

## OAuth Scope Matrix

Request only the permissions needed for enabled behavior. Twitch's [scope reference](https://dev.twitch.tv/docs/authentication/scopes/) remains authoritative.

| Authorized account | Scope | Current use | Requirement |
| --- | --- | --- | --- |
| Bot | `chat:read` | Receive IRC chat, commands, First Chatter input, and IRC notices | Required for current chat behavior |
| Bot | `chat:edit` | Send IRC messages and responses | Required for outgoing chat |
| Bot | `moderator:manage:banned_users` | Bot-authorized timeout/Nuke calls | Conditional; bot must also be a channel moderator |
| Broadcaster | `bits:read` | `channel.cheer` EventSub | Required for the configured cheer subscription |
| Broadcaster | `channel:read:redemptions` | Read redemption add/update events | Either this or `channel:manage:redemptions` for event receipt |
| Broadcaster | `channel:manage:redemptions` | Redemption events plus cancellation/status changes | Required when cancellation/refund attempts are used; replaces the read-only scope |
| Broadcaster | `channel:read:hype_train` | Hype Train begin/progress/end EventSub | Required for those subscriptions |
| Broadcaster | `moderator:read:followers` | `channel.follow` v2 EventSub | Required for follow subscription; broadcaster is its own qualifying moderator identity |
| Broadcaster | `moderation:read` | Get channel moderators | Required for current moderator-list lookup; `channel:manage:moderators` is an accepted higher-privilege alternative |
| Broadcaster | `channel:read:vips` | Get channel VIPs | Required for current VIP lookup; `channel:manage:vips` is an accepted higher-privilege alternative |
| Broadcaster | `moderator:manage:banned_users` | Broadcaster-authorized timeout path | Conditional on moderation behavior |

Public user, stream, channel, video, game, and profile-image lookups do not add another feature-specific scope, but the repository still sends the configured Client ID and broadcaster user token.

## EventSub WebSocket Setup

When Twitch sends `session_welcome`, the service reads the WebSocket session ID and attempts to register these seven subscriptions:

WebSocket subscriptions use the broadcaster user access token in `Auth:AccessToken`; the current implementation does not use an app access token for them.

| Subscription | Version | Condition | Scope |
| --- | ---: | --- | --- |
| `channel.cheer` | 1 | `broadcaster_user_id` | `bits:read` |
| `channel.channel_points_custom_reward_redemption.add` | 1 | `broadcaster_user_id` | `channel:read:redemptions` or `channel:manage:redemptions` |
| `channel.channel_points_custom_reward_redemption.update` | 1 | `broadcaster_user_id` | `channel:read:redemptions` or `channel:manage:redemptions` |
| `channel.hype_train.begin` | 2 | `broadcaster_user_id` | `channel:read:hype_train` |
| `channel.hype_train.progress` | 2 | `broadcaster_user_id` | `channel:read:hype_train` |
| `channel.hype_train.end` | 2 | `broadcaster_user_id` | `channel:read:hype_train` |
| `channel.follow` | 2 | `broadcaster_user_id`, `moderator_user_id` | `moderator:read:followers` |

`broadcaster_user_id` comes from `Twitch:BroadcasterUserId`. For `channel.follow` v2, the service validates `Auth:AccessToken` and uses the returned token user ID as `moderator_user_id`. In the recommended setup, that token belongs to the broadcaster and both IDs are the broadcaster ID.

Successful and failed subscription requests are logged. HTTP 409 is reported as already subscribed. Other failures include the HTTP status and response body, after which the service continues with the remaining subscription types. A failed token validation skips the follow subscription while the other subscription attempts continue.

Two registered subscriptions currently lack notification handlers:

- `channel.channel_points_custom_reward_redemption.update`
- `channel.hype_train.progress`

They are reported through the unhandled-subscription path rather than producing their intended application behavior.

The reconnect implementation also differs from Twitch's current guidance: it cancels the old connection before completing the reconnect connection and runs subscription registration after the new welcome. Twitch's documented reconnect sequence keeps the old connection until the replacement is ready and transfers existing subscriptions. A short delivery gap or redundant subscription attempts are therefore possible. Revocation messages are not explicitly handled, and comprehensive notification-ID deduplication is not implemented.

## Helix Integration

The general `twitch-helix` HTTP client uses `Auth:AccessToken` and `Auth:TwitchClientId`. It performs user/profile-image, stream, channel, video, game, moderator, and VIP lookups and supports broadcaster-authorized operations.

The `twitch-bot-helix` client uses `Auth:OAuthBearerBot` with the same `Auth:TwitchClientId`. It supports moderation operations performed as the bot. For a timeout to succeed, the token's account must match the submitted moderator ID, possess `moderator:manage:banned_users`, and be the broadcaster or a moderator in the channel.

Moderator and VIP endpoints require broadcaster-aligned authorization. Both current repository lookups follow Twitch's returned pagination cursors until no additional page remains.

Channel-point redemption status changes use the broadcaster token and require `channel:manage:redemptions`. Twitch also requires the reward to have been created by the same developer application identified by `Auth:TwitchClientId`. A reward created manually in Twitch or by a different application can produce valid EventSub notifications while still rejecting this application's cancellation/status-change request.

All Helix requests require the `Client-Id` header to identify the same application for which the access token was issued. A valid token paired with the wrong Client ID is not a valid configuration.

## First-Run Verification

Use a private development channel or a low-impact verification window where practical.

- [ ] Confirm `appsettings.json` is beside the executable being launched, with any optional `appsettings.Development.json` override beside it.
- [ ] Validate the raw bot and broadcaster tokens; confirm Client ID, login, numeric user ID, scopes, and expiry.
- [ ] Confirm the bot username and ID belong to the bot token owner.
- [ ] Confirm the channel login and broadcaster ID belong to the broadcaster token owner.
- [ ] Start the application and review startup messages without copying credentials into logs or screenshots.
- [ ] Start the bot runtime and confirm the IRC connection succeeds.
- [ ] Confirm the bot joins `Twitch:Channel`.
- [ ] Send an ordinary message and confirm incoming chat is observed.
- [ ] Trigger a safe bot response and confirm outgoing chat works.
- [ ] Exercise a configured command with a non-destructive result.
- [ ] Verify First Chatter behavior with an account and stream state appropriate for the test.
- [ ] Confirm the EventSub WebSocket receives `session_welcome`.
- [ ] Confirm all intended subscription registrations succeed; investigate 401, 403, or other failures individually.
- [ ] Confirm user/profile and moderator/VIP Helix lookups return expected results.
- [ ] Trigger only safe, intended alerts. Paid Bits, subscriptions, and provider activity can have real consequences or charges.
- [ ] If testing redemption cancellation, use a reward created by the same Twitch developer application.

> **Warning:** Do not assume the application's Testing tab is isolated from production persistence, alert history, chat output, media, or other runtime state. Back up mutable JSON data and use test identities/data where necessary.

## Troubleshooting

| Symptom | Checks and corrective action |
| --- | --- |
| Incorrect Client ID / 401 | Validate the token and confirm its `client_id` exactly matches `Auth:TwitchClientId`. Renew an expired or revoked token and restart. |
| IRC authentication or connection failure | Confirm `Twitch:BotUsername` matches the token owner, `Auth:OAuthToken` begins with exactly `oauth:`, and the token includes `chat:read` and `chat:edit`. |
| Bot receives but cannot send chat | Confirm `chat:edit` was authorized and the bot joined the expected `Twitch:Channel`. |
| Bot username or bot ID mismatch | Resolve the bot login through Get Users or token validation and correct both `Twitch:BotUsername` and `Twitch:BotUserId`. |
| Wrong channel or broadcaster ID | Resolve the broadcaster login through Get Users and align `Twitch:Channel`, `Twitch:BroadcasterUserId`, and the broadcaster token owner. |
| Missing token at startup | Populate the exact required key. Placeholder text may pass presence validation but cannot authenticate. |
| Bearer token fails | Store only the raw access token in `Auth:OAuthBearerBot` and `Auth:AccessToken`; do not include `Bearer ` or `oauth:`. |
| 401 Unauthorized | Validate token expiry, revocation, owner, and Client ID. Replace the token through the official OAuth flow; the application cannot refresh it. |
| 403 Forbidden | Check the endpoint/event scope and whether the authorizing account has the required broadcaster or moderator role. Do not solve a 403 by granting unrelated scopes. |
| EventSub subscription failure | Check the logged subscription type/status, broadcaster ID, token owner, required scope, and WebSocket session. A validation failure specifically prevents the follow subscription. |
| EventSub connects but an alert is missing | Confirm the event was successfully subscribed, has a notification handler, and has configured media. Redemption-update and Hype Train progress notifications are currently unhandled. |
| EventSub problems after reconnect | Restart if subscriptions do not recover. The current reconnect sequence can create a delivery gap or redundant registrations. |
| Helix lookup fails | Confirm raw bearer format, Client ID/token pairing, token validity, and any endpoint-specific scope or ownership requirement. |
| Timeout/Nuke fails | Confirm the selected token has `moderator:manage:banned_users`, its user ID matches the moderator ID sent, and that account is the broadcaster or a channel moderator. |
| Redemption status change fails | Confirm `channel:manage:redemptions`, broadcaster ownership, Client ID pairing, and that the same developer application created the reward. |
| Configuration seems ignored | Put the files beside the executable. `appsettings.Development.json` overrides matching base values. Environment variables and command-line arguments are not application-setting providers. Restart after edits. |

Never place credentials in logs, screenshots, pull requests, issues, or troubleshooting transcripts. If a credential may have been exposed, revoke and replace it.

## Known Limitations

- OAuth tokens are generated by an external official/trusted flow; TwitchChatBot has no token-generation UI.
- There is no in-application OAuth callback or authorization-code exchange.
- Token refresh is not implemented, and the configured refresh-token accessor is unused.
- Comprehensive startup/hourly validation of every token is not implemented.
- Twitch Chat API migration has not been implemented; current chat behavior still uses TwitchLib/IRC.
- Channel-point redemption update and Hype Train progress are subscribed but have no notification handlers.
- EventSub reconnect sequencing differs from current Twitch guidance.
- EventSub revocation messages are not explicitly handled.
- Comprehensive EventSub notification deduplication is not implemented.
- IRC membership events are not a complete viewer-list source for large channels.
- Testing-tab isolation from production persistence and runtime side effects is not guaranteed.

## Official Twitch Documentation

- [Registering Your App](https://dev.twitch.tv/docs/authentication/register-app)
- [Getting OAuth Access Tokens](https://dev.twitch.tv/docs/authentication/getting-tokens-oauth)
- [Twitch Access Token Scopes](https://dev.twitch.tv/docs/authentication/scopes/)
- [Validating Tokens](https://dev.twitch.tv/docs/authentication/validate-tokens/)
- [Refreshing Access Tokens](https://dev.twitch.tv/docs/authentication/refresh-tokens/)
- [Twitch CLI](https://dev.twitch.tv/docs/cli/)
- [Twitch CLI Token Command](https://dev.twitch.tv/docs/cli/token-command/)
- [Twitch IRC](https://dev.twitch.tv/docs/chat/irc/)
- [EventSub WebSocket Events](https://dev.twitch.tv/docs/eventsub/handling-websocket-events)
- [Managing EventSub Subscriptions](https://dev.twitch.tv/docs/eventsub/manage-subscriptions/)
- [EventSub Subscription Types](https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types)
- [Twitch Helix API Reference](https://dev.twitch.tv/docs/api/reference)

## Repository References

- [README](../README.md)
- [Configuration Reference](CONFIGURATION.md)
- [Feature History](../FEATURE_HISTORY.md)

