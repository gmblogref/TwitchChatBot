# Streamlabs Donation Setup

This guide documents the Streamlabs integration implemented by the current TwitchChatBot repository. It covers the provider connection and donation path without replacing the complete key inventory in [Configuration Reference](CONFIGURATION.md).

Streamlabs is optional. TwitchChatBot can run without it when either `Streamlabs:SocketUrl` or `Streamlabs:SocketToken` is blank. The separate `MediaMapFiles:DonationMedia` path remains required by startup validation even when the provider is disabled.

## Current Integration

TwitchChatBot uses `SocketIOClient` 4.0.3 to connect to a Streamlabs Socket.IO endpoint. It does not implement a Streamlabs webhook receiver, donation polling, an OAuth callback, or an in-application authorization flow.

The runtime path is:

```text
Start bot runtime
  -> start Streamlabs Socket.IO provider
  -> receive an "event" message
  -> accept events whose type is "donation"
  -> parse each item in the message array
  -> select donation media
  -> add alert history
  -> enqueue the visual alert
  -> request donation speech through AWS Polly
```

The provider is started after Twitch chat and EventSub when `ChatBotController.StartAsync()` runs. Merely opening the Windows Forms application does not connect to Streamlabs.

## Obtain a Socket Token

Streamlabs' official [Socket API](https://dev.streamlabs.com/v1/docs/socket-api) documents a socket token obtained from the `/socket/token` API and passed to `https://sockets.streamlabs.com` as the `token` query value. The documented socket-token endpoint requires an API access token, and the current scope list identifies `socket.token` for that endpoint.

The public Socket API page is currently labeled **v1.0**, while Streamlabs also publishes newer OAuth documentation. Confirm the current supported authorization and socket-token procedure in the Streamlabs developer portal before creating credentials. TwitchChatBot does not automate any part of this process.

At a high level, the externally managed process is:

1. Register or use a Streamlabs developer application according to the current Streamlabs requirements.
2. Complete the official Streamlabs account-authorization process outside TwitchChatBot.
3. Request only the scopes required to obtain the socket token, including `socket.token` where required by the current API.
4. Exchange the authorization code for an API access token using Streamlabs' official process.
5. Use the official `/socket/token` endpoint to obtain the socket credential.
6. Store only the resulting socket credential in ignored local TwitchChatBot configuration.

See Streamlabs' official [account connection](https://dev.streamlabs.com/docs/connecting-to-an-account), [access-token](https://dev.streamlabs.com/docs/obtain-an-access_token), and [scope](https://dev.streamlabs.com/docs/scopes) documentation. Do not paste an access token, socket token, client secret, or authorization code into documentation, source control, issues, screenshots, or an AI-assistant conversation.

## Configure TwitchChatBot

Place private values in ignored `TwitchChatBot/appsettings.json` or `TwitchChatBot/appsettings.Development.json` beside the executable at runtime. A minimal Development override has this shape:

```json
{
  "Streamlabs": {
    "SocketUrl": "https://sockets.streamlabs.com",
    "SocketToken": "YOUR_PRIVATE_STREAMLABS_SOCKET_TOKEN"
  }
}
```

The implemented keys are:

| Key | Requirement | Secret | Current use |
| --- | --- | --- | --- |
| `Streamlabs:SocketUrl` | Conditional | No | Base Socket.IO endpoint. |
| `Streamlabs:SocketToken` | Conditional | Yes | Appended as the `token` query value. |
| `MediaMapFiles:DonationMedia` | Required at startup | No | Selects the donation media-map JSON file. |
| `TTS:Voices:Follow` | Required at startup | No | Also serves as the donation speech voice; no donation-specific voice key exists. |

Both Streamlabs socket values must be nonblank to enable the provider. There is no separate `Enabled` setting. Leaving either value blank is the supported way to omit Streamlabs.

The complete precedence, validation, and commit-policy rules are in [Configuration Reference](CONFIGURATION.md#streamlabs).

## Configure Donation Media

`MediaMapFiles:DonationMedia` points to runtime JSON data rather than another configuration provider. Its current shape is:

```json
{
  "default": "C:\\TwitchChatBotMedia\\donations\\default.mp4",
  "tiers": [
    {
      "Min": 10.00,
      "Media": "C:\\TwitchChatBotMedia\\donations\\tier-10.mp4"
    }
  ]
}
```

For a donation, the application selects the qualifying tier with the highest `Min` value. If none qualifies, it uses `default`. If no media path is available, the text-only visual alert is still enqueued.

Missing, empty, malformed, or inaccessible donation-map data is logged and represented by an empty map. It does not enable or disable the provider.

## Donation Event Processing

The handler expects a Streamlabs event payload with:

- Root `type` equal to `donation`, case-insensitively.
- Root `message` containing an array.
- Each array item optionally containing `name`, `amount`, and `message`.

This matches the donation shape shown by the official Streamlabs Socket API. The current parser expects `amount` as a JSON string and parses it using the machine's current culture. Unexpected payload or numeric formats can cause an event to be dropped.

Each accepted donation:

- Displays donor name and amount in a visual alert.
- Adds the donor's supplied message when present.
- Adds an in-memory donation history entry.
- Requests TTS for the donor message, or a generated donation sentence when no message was supplied.

Donation speech requires the separately configured [AWS Polly integration](AWS_POLLY_SETUP.md). A Streamlabs donation can still produce its visual alert when Polly credentials are unavailable.

## Connectivity and Safe Verification

Use a private or low-impact Streamlabs test environment where possible.

1. Confirm the private configuration files are beside the executable being launched.
2. Confirm both socket settings are nonblank without printing their values.
3. Confirm `MediaMapFiles:DonationMedia` points to readable JSON and its media files are under the configured media root.
4. Start the bot runtime.
5. Look for the Streamlabs provider-start and connection messages.
6. Use an official Streamlabs test-donation mechanism if one is available for the connected account.
7. Confirm the donation appears in alert history and in the browser/OBS alert overlay.
8. If AWS Polly is configured, confirm the donation message is synthesized separately.

The Testing tab's donation button exercises `DonationAlertService` directly. It can verify media selection, alerting, history, and TTS, but it does **not** verify Streamlabs authentication, Socket.IO connectivity, or live payload compatibility.

## Failure Behavior and Troubleshooting

| Symptom | Current behavior and checks |
| --- | --- |
| Provider says it is disabled | One or both socket settings are blank. Populate both only if Streamlabs is intended. |
| Invalid URL warning | Confirm `Streamlabs:SocketUrl` is an absolute Socket.IO endpoint. |
| No connection | Recheck the current Streamlabs endpoint, socket-token validity, account authorization, and required scope. |
| Connected but donations are absent | Confirm the provider emits an `event` whose `type` is `donation` and whose `message` is an array. |
| Event-processing error | A malformed item, non-string amount, or unexpected payload shape can drop that event. |
| Visual alert has no media | Check donation-map JSON, tier thresholds, file paths, media-root mapping, and overlay connectivity. |
| Visual alert works but speech does not | Troubleshoot [AWS Polly](AWS_POLLY_SETUP.md); Streamlabs and Polly credentials are independent. |
| Connection stops after a failure | Stop and restart the bot runtime after correcting configuration. The application has no provider-specific restart loop. |

The SocketIOClient project's documentation describes default polling with WebSocket upgrade and library-level reconnection defaults. TwitchChatBot does not configure those options or handle reconnection-failed events, so uninterrupted reconnection is not guaranteed by this application. See the [SocketIOClient project documentation](https://github.com/doghappy/socket.io-client-csharp/wiki) for the package's transport and option behavior.

## Security and Cost Notes

- Treat the socket token, Streamlabs API access token, authorization code, and client secret as private credentials.
- Keep credentials only in ignored local configuration or the provider's secure tooling.
- Inspect local build output before sharing it because private configuration is copied beside local executables.
- Rotate or revoke credentials if they may have been exposed.
- Review Streamlabs' current service terms and any provider-side charges or account requirements before testing.
- Real donations have financial consequences. Prefer official test facilities over sending a real payment.

## Known Limitations

- No in-application Streamlabs OAuth or token acquisition exists.
- No webhook or donation-polling implementation exists.
- The live provider was not independently revalidated during the .NET 10 modernization.
- Current public socket documentation is labeled v1.0, so current service compatibility must be checked manually.
- Reconnection settings are inherited from the SocketIOClient package and are not controlled by TwitchChatBot.
- Payload parsing assumes a message array and string-formatted amount.
- A malformed item can stop processing later items in the same event.
- Donation history entries currently have no donation replay case in `AlertReplayService`.
- Donation TTS uses the follow voice rather than a dedicated donation voice.

## Related Documentation

- [README](../README.md)
- [Configuration Reference](CONFIGURATION.md)
- [AWS Polly Setup](AWS_POLLY_SETUP.md)
- [Feature History](../FEATURE_HISTORY.md)
