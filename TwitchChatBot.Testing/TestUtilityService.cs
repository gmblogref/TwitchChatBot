using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

public class TestUtilityService : ITestUtilityService
{
    private readonly ITwitchAlertTypesService _twitchAlertTypesService;
    private readonly IFirstChatterAlertService _firstChatterAlertService;
    private readonly ICommandAlertService _commandAlertService;
    private readonly ITtsService _tsService;
    private readonly ITwitchClientWrapper _twitchClient;

    public TestUtilityService(
        ITwitchAlertTypesService twitchAlertTypesService,
        IFirstChatterAlertService firstChatterAlertService,
        ICommandAlertService commandAlertService,
        ITwitchClientWrapper twitchClient,
        ITtsService tsService)
    {
        _twitchAlertTypesService = twitchAlertTypesService;
        _firstChatterAlertService = firstChatterAlertService;
        _commandAlertService = commandAlertService;
        _twitchClient = twitchClient;
        _tsService = tsService;
    }

    // Twitch Alert Tests
    public async Task TriggerChannelPointTestAsync(string redemption) =>
        await _twitchAlertTypesService.HandleChannelPointRedemptionAsync("TestUser", redemption);
    public async Task TriggerCheerTestAsync(int bits) =>
        await _twitchAlertTypesService.HandleCheerAsync("TestUser", bits, "Test cheer message");

    public void TriggerFollowTest(string userName) =>
        _twitchAlertTypesService.HandleFollowAsync(userName);
    public async Task TriggerHypeTrainTestAsync() =>
        await _twitchAlertTypesService.HandleHypeTrainAsync();

    public async Task TriggerRaidTestAsync(string userName, int viewers) =>
        await _twitchAlertTypesService.HandleRaidAsync(userName, viewers);

    public async Task TriggerReSubTestAsync(string userName, int months) =>
        await _twitchAlertTypesService.HandleResubAsync(userName, months, $"{months} months that's almost a year", "1000");
    
    public async Task TriggerSubGiftTestAsync(string userName, string recipient) =>
        await _twitchAlertTypesService.HandleSubGiftAsync(userName, recipient, "2000");

    public async Task TriggerSubMysteryGiftTestAsync(string userName, int subs) =>
        await _twitchAlertTypesService.HandleSubMysteryGiftAsync(userName, subs, "3000");
    public async Task TriggerSubTestAsync(string userName) =>
        await _twitchAlertTypesService.HandleSubscriptionAsync(userName, "");

    public async Task TriggerWatchStreakUserNoticeTestAsync(string username, int streakCount) =>
        await _twitchAlertTypesService.HandleWatchStreakNoticeAsync(username, streakCount, "This is a test watch streak share!");
    
    // Commands Test
    public async Task TriggerCommandTestAsync(string command) =>
        await _commandAlertService.HandleCommandAsync(
            "!" + command, 
            AppSettings.TWITCH_BOT_ID!,
            "TestUser",
            AppSettings.TWITCH_CHANNEL!,
            (channel, message) => _twitchClient.SendMessage(channel, message));

    // First Chatter Test
    public async Task TriggerFirstChatTestAsync(string username) =>
        await _firstChatterAlertService.HandleFirstChatAsync(AppSettings.TWITCH_BOT_ID!, username, username);

    public void TriggerFirstChatClear() =>
        _firstChatterAlertService.ClearFirstChatters();

    public async Task TriggerTextToSpeech(string message, string speaker) =>
        await _tsService.SpeakAsync(message, speaker, null);
}