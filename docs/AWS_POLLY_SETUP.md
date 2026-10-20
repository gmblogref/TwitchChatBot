# AWS Polly Text-to-Speech Setup

This guide documents the Amazon Polly integration implemented by the current TwitchChatBot repository. Polly is the only implemented TTS engine; changing `TTS:Engine` does not select another provider.

AWS credentials are not required for the WinForms application to start. The TTS configuration and writable media paths are still required, and speech is skipped when the AWS SDK cannot resolve credentials.

## Current Integration

TwitchChatBot references `AWSSDK.Polly` 4.0.6.9 and resolves `AWSSDK.Core` 4.0.4. `TtsService`:

1. Creates a `text_to_speech` directory below `MediaBase:TwitchAlertsFolder`.
2. Resolves AWS credentials with the AWS SDK v4 default credentials resolver.
3. Queues speech requests while credentials are available.
4. Calls Polly `SynthesizeSpeech` with plain text, MP3 output, and the neural engine.
5. Retries once with the standard engine only when Polly reports a neural-engine problem.
6. Writes a temporary MP3 locally.
7. Enqueues `/media/text_to_speech/<file>` through the normal alert overlay pipeline.
8. Deletes generated files best-effort when `TtsService` is disposed.

No speech is generated in the browser, by Windows speech APIs, or by OpenAI. Polly is the only current synthesis mechanism.

## AWS Account and Credentials

Create or use an AWS account under the developer's control. Do not place AWS credentials in `appsettings.json`; TwitchChatBot has no application configuration keys for an AWS access key, secret key, session token, or profile.

The application constructs `AmazonPollyClient` without explicit credentials. AWS's official [.NET SDK v4 credential-resolution documentation](https://docs.aws.amazon.com/sdk-for-net/v4/developer-guide/creds-assign.html) defines this search order:

1. Credentials explicitly assigned to the service client. TwitchChatBot does not supply these.
2. Session credentials from `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, and `AWS_SESSION_TOKEN`.
3. Basic credentials from `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY`.
4. Web-identity credentials from `AWS_WEB_IDENTITY_TOKEN_FILE` and `AWS_ROLE_ARN`.
5. A profile selected through `AWSConfigs.AWSProfileName`. TwitchChatBot does not set this property.
6. A profile selected through `AWS_PROFILE`.
7. The `[default]` profile.
8. Container credentials.
9. Amazon EC2 instance metadata.

For ordinary Windows development, use an appropriately protected shared AWS credentials profile or process/user environment variables. Prefer temporary credentials where practical. If selecting a non-default profile, set `AWS_PROFILE` in the environment that launches TwitchChatBot; there is no TwitchChatBot profile setting.

The shared credentials file is normally under the developer profile at `%USERPROFILE%\.aws\credentials`. Never copy that file into the repository or application output.

## Least-Privilege IAM Policy

Repository code invokes only `SynthesizeSpeechAsync`. It does not call `DescribeVoices`, lexicon-management APIs, asynchronous synthesis tasks, S3, or SNS.

The minimum application permission is therefore `polly:SynthesizeSpeech`, confirmed by AWS's [Amazon Polly service authorization reference](https://docs.aws.amazon.com/service-authorization/latest/reference/list_polly.html):

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "AllowTwitchChatBotSpeech",
      "Effect": "Allow",
      "Action": "polly:SynthesizeSpeech",
      "Resource": "*"
    }
  ]
}
```

Apply permissions to a dedicated identity appropriate for the developer's environment. Do not grant `AdministratorAccess`, broad `polly:*`, S3, or unrelated permissions for TwitchChatBot.

## Application Configuration

The complete TTS key descriptions are in [Configuration Reference](CONFIGURATION.md#text-to-speech-and-aws-polly). The primary settings are:

| Key or group | Requirement | Current use |
| --- | --- | --- |
| `TTS:PollyRegion` | Required | Passed to `RegionEndpoint.GetBySystemName`. |
| `TTS:DefaultSpeaker` | Required | Default voice when no allowed override is selected. |
| `TTS:MaxChars` | Required, integer at least 1 | Limits sanitized speech text. |
| `TTS:AllowedVoices` | Required, nonempty | Case-insensitive allowlist for requested voice overrides. |
| `TTS:Voices:*` | Required event-specific values | Supplies voices for Twitch alerts, channel points, watch streaks, and donations. |
| `TTS:Templates:*` | Required | Supplies non-AI fallback and normal-event speech text. |
| `MediaBase:TwitchAlertsFolder` | Required | Parent of temporary `text_to_speech` MP3 output and the overlay `/media` mapping. |
| `TTS:Engine` | Unused | Does not change the provider; Polly remains hard-wired. |

`TTS:Voices:Follow` also controls Streamlabs donation speech because there is no donation-specific voice key.

Choose voices supported by the configured region and engine. AWS maintains the authoritative [available-voices table](https://docs.aws.amazon.com/polly/latest/dg/available-voices.html) and [neural voice/region compatibility](https://docs.aws.amazon.com/polly/latest/dg/neural-voices.html).

The application attempts neural synthesis first. If the Polly exception message indicates a neural problem, it retries that item once with the standard engine. This fallback cannot help when the chosen voice is unavailable in both engines or the region, credentials, permission, request, or service is invalid.

Event voice overrides should also appear in `TTS:AllowedVoices`. When an override is not allowed, the service uses the default voice and prepends the rejected override text to the spoken sentence.

## Features That Use TTS

Current speech callers include:

- Twitch cheers, follows, raids, subscriptions, resubscriptions, gifts, watch streaks, and Hype Train completion.
- Configured text channel-point rewards.
- Streamlabs donations.
- `!tts` and `!birthday` command paths.
- Selected moderation/Nuke behavior.
- Manual Testing-tab TTS.
- Supported alert replay paths.
- OpenAI-generated raid, streak, gift-bundle, and annual-resub text.

See [OpenAI Alert-Text Setup](OPENAI_SETUP.md) for the event-specific AI speech fallback limitations and [Streamlabs Donation Setup](STREAMLABS_SETUP.md) for donation handling.

## Startup, Queue, and Failure Behavior

`TtsService` is a singleton created while the application dependency graph is built. It immediately creates the output directory and starts a background runner.

- Missing AWS credentials do not prevent application startup.
- Missing required TTS keys do prevent startup through application configuration validation.
- An inaccessible or invalid media output path can fail TTS construction and prevent startup.
- Credential discovery failure is caught, logged, and rechecked periodically.
- A speech request received while credentials are unavailable is skipped rather than retained.
- The request that initiates an opportunistic credential recheck is still skipped.
- A failed synthesis item is logged and dropped; it is not requeued.
- Failure does not cancel visual alerts that were already enqueued or unrelated bot features.
- Neural incompatibility alone receives the one-time standard-engine retry.

The queue can be skipped or reset from the UI. Disposal cancels the runner and removes generated TTS files best-effort.

## Safe Verification

1. Confirm the AWS identity has only `polly:SynthesizeSpeech` plus any permissions required by its external credential mechanism.
2. Confirm the AWS SDK can discover credentials in the environment that launches TwitchChatBot.
3. Confirm `TTS:PollyRegion` is a current Polly region.
4. Confirm the default and event voices are supported in that region and are listed in `TTS:AllowedVoices`.
5. Confirm `MediaBase:TwitchAlertsFolder` exists or can be created and is writable.
6. Start the application and check that credential discovery reports success without displaying credential values.
7. Use the Testing tab with short, non-sensitive text.
8. Confirm an MP3 is produced under `text_to_speech` and played through the connected alert overlay.
9. Confirm the test appears in AWS usage and cost reporting.

Do not test with private, regulated, or credential-bearing text. TTS input is sent to AWS for synthesis and a preview is written to application logs.

## Troubleshooting

| Symptom | Current behavior and checks |
| --- | --- |
| AWS credentials not found | Check environment-variable completeness, `AWS_PROFILE`, shared credential file availability, profile name, and the launch process's user context. |
| Credentials are configured but empty | Remove incomplete variables or replace them through the developer's secure credential process. |
| Access denied | Confirm the resolved identity allows `polly:SynthesizeSpeech`; do not add unrelated permissions. |
| Invalid security token/signature | Refresh temporary credentials and confirm all required session fields are present. |
| Voice or engine rejected | Check voice, engine, and region compatibility. Neural-specific errors receive one standard retry. |
| Speech request is skipped after credentials are fixed | Wait for the periodic recheck or restart the application; the already skipped request is not replayed automatically. |
| MP3 cannot be written | Check the media-root path, directory permissions, available disk space, and security software. |
| MP3 exists but is not heard | Confirm the overlay client is connected and `/media/text_to_speech/...` maps to the same physical media root. |
| Visual alert appears without speech | Check credentials, IAM permission, region, voice, Polly availability, and output path. The visual path is intentionally isolated from synthesis failure. |

## Pricing, Budgets, and Billing Alerts

Amazon Polly is a metered AWS service. AWS's current [Polly pricing](https://aws.amazon.com/polly/pricing/) charges by characters processed, with price differences by voice engine and any applicable free-tier or promotional terms.

Configure cost controls separately in the AWS account; TwitchChatBot cannot create or enforce them. AWS documents how to create a [cost budget and alert thresholds](https://docs.aws.amazon.com/cost-management/latest/userguide/create-cost-budget.html). Consider:

- A conservative monthly cost budget.
- Actual-spend and forecasted-spend notifications.
- Cost Anomaly Detection where appropriate.
- Regular review of Polly usage after testing.

Budget and billing notifications are monitoring tools, not guaranteed instantaneous cutoffs. The developer remains responsible for AWS account security and charges.

## Security Guidance

- Never place AWS credentials in application JSON, source code, documentation, tests, screenshots, or chat transcripts.
- Do not commit `.aws` credential/config files.
- Prefer temporary credentials and dedicated least-privilege identities.
- Restrict local credential-file permissions to the developer account.
- Rotate credentials immediately after suspected exposure.
- Inspect application logs before sharing them because TTS previews can contain user-supplied text.
- Inspect build output before sharing it because other private application configuration can be copied beside local executables.

## Known Limitations

- Polly is the only implemented TTS engine; `TTS:Engine` is unused.
- There is no TTS enable/disable setting.
- TTS configuration and the output path remain required without AWS credentials.
- Credential availability is checked, but credentials are not proactively verified by a harmless Polly request.
- Speech received while credentials are unavailable is dropped.
- Failed synthesis items are not retried except for the neural-to-standard case.
- Voice and region compatibility is not validated during startup.
- The application does not create IAM identities, credentials, budgets, billing alarms, or anomaly monitors.
- Existing unit tests do not exercise live credentials, Polly synthesis, MP3 output, or neural fallback.

## Related Documentation

- [README](../README.md)
- [Configuration Reference](CONFIGURATION.md)
- [OpenAI Alert-Text Setup](OPENAI_SETUP.md)
- [Streamlabs Donation Setup](STREAMLABS_SETUP.md)
- [Feature History](../FEATURE_HISTORY.md)
