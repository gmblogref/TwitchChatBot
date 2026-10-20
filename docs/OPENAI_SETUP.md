# OpenAI Alert-Text Setup

This guide documents the optional OpenAI behavior implemented by the current TwitchChatBot repository. The application uses direct HTTP requests to the OpenAI Responses API; it does not use an OpenAI SDK.

For the full key inventory and configuration precedence, see [Configuration Reference](CONFIGURATION.md#openai-alert-text).

## Current Integration

`AiTextService` uses a named `HttpClient` to send:

```http
POST <OpenAI:BaseUrl>/responses
Authorization: Bearer <OpenAI:ApiKey>
Content-Type: application/json
```

The request body contains the configured model and an application-built text prompt:

```json
{
  "model": "YOUR_SUPPORTED_MODEL",
  "input": "APPLICATION_GENERATED_PROMPT"
}
```

The service searches the response for an `output_text` content block, trims it, and limits it to `OpenAI:DefaultAlertMaxWords`. Requests have a 30-second HTTP timeout and no application retry.

The visual alert and alert-history entry are created before AI generation. OpenAI output is used only as text sent to the existing AWS Polly speech service.

## Create and Protect an API Key

Create and manage a project API key through OpenAI's official [API key page and guidance](https://help.openai.com/en/articles/4936850-where-do-i-find-my-openai-api-key). A ChatGPT subscription does not supply API billing; OpenAI documents [ChatGPT and API billing as separate systems](https://help.openai.com/en/articles/9039756-managing-billing-for-chatgpt-and-the-api-platform).

OpenAI recommends secure storage, per-user or appropriately scoped project keys, rotation, usage monitoring, and never committing keys. See [Best Practices for API Key Safety](https://help.openai.com/en/articles/5112595-best-practices-for-api-key-safety).

TwitchChatBot does not load application settings from environment variables. For this repository, store the key in ignored `appsettings.json` or `appsettings.Development.json`; do not set `OPENAI_API_KEY` and assume the application will read it.

## Configure TwitchChatBot

A safe Development override can contain:

```json
{
  "OpenAI": {
    "ApiKey": "YOUR_PRIVATE_OPENAI_API_KEY"
  }
}
```

The base configuration must retain all current OpenAI settings:

| Key | Requirement | Secret | Current use |
| --- | --- | --- | --- |
| `OpenAI:BaseUrl` | Required | No | Base address for the named HTTP client. Keep the trailing slash for relative `responses` requests. |
| `OpenAI:ApiKey` | Optional | Yes | Bearer credential. Blank values do not stop startup but make generation unsuccessful. |
| `OpenAI:Model` | Required | No | Sent as the Responses API `model` field. |
| `OpenAI:DefaultAlertTone` | Required | No | Stored on `AlertContext`, but current prompt builders do not use it. |
| `OpenAI:DefaultAlertMaxWords` | Required, integer at least 1 | No | Included in prompt rules and enforced by local word truncation. |
| `OpenAI:WatchStreakThreshold` | Required, integer at least 0 | No | Minimum qualifying watch-streak count. |
| `OpenAI:GiftSubThreshold` | Required, integer at least 0 | No | Minimum qualifying gift-bundle count. |

There is no independent OpenAI `Enabled` Boolean. Leaving `OpenAI:ApiKey` blank prevents successful authenticated generation, but the client and qualifying generation paths still exist. The other OpenAI settings remain startup-required.

`OpenAI:DefaultAlertTone` is currently configured and copied into alert contexts, but it does not affect generated prompts. Changing it has no confirmed output effect.

## Alert Types and Prompt Data

| Alert path | When AI is attempted | Data sent in the generated prompt | Speech fallback if generation fails |
| --- | --- | --- | --- |
| Raid | Every raid | Raider username, viewer count, maximum-word rule | **Yes.** Uses `TTS:Templates:Raid`. |
| Watch streak | Streak at or above `OpenAI:WatchStreakThreshold` | Viewer username, streak count, maximum-word rule | **No** for qualifying streaks. The visual alert still continues. |
| Gift-sub bundle | Count at or above `OpenAI:GiftSubThreshold` | Gifter username, gift count, tier, maximum-word rule | **No** for qualifying bundles. The visual alert still continues. |
| Annual resubscription milestone | At least 12 months and evenly divisible by 12 | Username, years calculated as months divided by 12, tier, maximum-word rule | **No.** The visual alert still continues. |

The current prompts do not send OAuth tokens, Twitch IDs, OpenAI credentials, or local file paths. They do send public-facing event information such as usernames, counts, and subscription tier. Review this data flow against the channel's privacy expectations before enabling the integration.

Nonqualifying gift bundles and watch streaks use their existing user-message/template speech paths without calling OpenAI. Non-annual resubscriptions likewise use the normal user-message or template path.

## Model, API, and Billing

The tracked example currently names `gpt-4o-mini`, but the application does not verify model availability. Confirm the configured model supports the Responses API in the current [OpenAI model catalog](https://developers.openai.com/api/docs/models) before use.

The repository implementation aligns with OpenAI's [Responses API guidance](https://developers.openai.com/api/docs/guides/migrate-to-responses): it posts a `model` and `input`, then reads generated output text. The response parser is application-specific and can return no text if the response structure does not contain the expected `output_text` block.

API use can incur charges based on the selected model and token usage. Review current [OpenAI API pricing](https://developers.openai.com/api/docs/pricing), configure API billing separately from ChatGPT, and set appropriate project or organization spend controls. OpenAI's current billing documentation notes that spend enforcement and balance processing may not be instantaneous.

## Safe Verification

1. Confirm `OpenAI:BaseUrl` is `https://api.openai.com/v1/` unless an explicitly supported endpoint is intended.
2. Confirm the selected model currently supports Responses API text generation.
3. Store a newly created key only in ignored local configuration.
4. Set conservative word and event thresholds before testing.
5. Start the application and bot runtime.
6. Trigger a safe raid test or another supported alert path without involving a paid Twitch action.
7. Confirm visual alert delivery separately from generated speech.
8. Review OpenAI usage and charges after the test.
9. Remove or rotate the key when it is no longer needed.

OpenAI-generated speech also requires working [AWS Polly setup](AWS_POLLY_SETUP.md). A successful OpenAI response cannot produce audio when Polly credentials or synthesis are unavailable.

## Failure Behavior and Troubleshooting

| Symptom | Current behavior and checks |
| --- | --- |
| Application fails before the main window | Confirm `OpenAI:BaseUrl` is an absolute URI and all required OpenAI settings and integers are present. |
| 401 or no generated text | Replace a missing, invalid, expired, revoked, or incorrectly scoped API key. The service returns null without logging the response body. |
| Billing or quota error | Check API billing, available credits, spend limits, project/model access, and current usage. |
| Unsupported model | Select a current Responses-compatible model; unsupported values return no generated line. |
| Timeout or provider outage | The request times out after 30 seconds, is not retried, and returns no generated line. |
| Raid still speaks template text | This is the implemented raid fallback. |
| Qualifying streak/gift/annual resub has visual alert but no speech | This is the current no-speech-fallback limitation after unsuccessful AI generation. |
| AI succeeds but there is no audio | Troubleshoot AWS credentials, Polly region/voice support, and the media output directory. |
| Tone changes do nothing | `OpenAI:DefaultAlertTone` is not read by the current prompt builders. |

## Security and Cost Notes

- Never commit or share the API key.
- Do not place the key in the tracked example, documentation, tests, screenshots, issues, or chat transcripts.
- Inspect Debug and Release output before sharing it because local private settings are copied beside the executable.
- Use a key with only the permissions needed for this application, monitor usage, and rotate it periodically.
- Treat usernames and event details sent in prompts as externally processed data.
- Review current model pricing and usage after testing; a ChatGPT subscription does not cover API usage.

## Known Limitations

- There is no explicit OpenAI enable/disable Boolean.
- All supporting OpenAI settings except the API key remain startup-required.
- `OpenAI:DefaultAlertTone` has no current prompt effect.
- There is no application retry, response-body error logging, or user-visible provider health indicator.
- Failed qualifying watch-streak, gift-bundle, and annual-resub generation has no speech fallback.
- The model is not checked for existence or Responses API support during startup.
- OpenAI output is used for TTS only, not for the visual alert text.
- OpenAI-generated speech still depends on AWS Polly.

## Related Documentation

- [README](../README.md)
- [Configuration Reference](CONFIGURATION.md)
- [AWS Polly Setup](AWS_POLLY_SETUP.md)
- [Feature History](../FEATURE_HISTORY.md)
