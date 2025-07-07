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

    public void TriggerFollowTest() =>
        _twitchAlertTypesService.HandleFollowAsync("TestUser");
    public async Task TriggerHypeTrainTestAsync() =>
        await _twitchAlertTypesService.HandleHypeTrainAsync();

    public async Task TriggerRaidTestAsync() =>
        await _twitchAlertTypesService.HandleRaidAsync("TestRaider", 20);

    public async Task TriggerReSubTestAsync() =>
        await _twitchAlertTypesService.HandleResubAsync("TestUser", 5, "5 months, that's almost a year");
    
    public async Task TriggerSubGiftTestAsync() =>
        await _twitchAlertTypesService.HandleSubGiftAsync("TestUser", "TestUser2");

    public async Task TriggerSubMysteryGiftTestAsync(int subs) =>
        await _twitchAlertTypesService.HandleSubMysteryGiftAsync("TestUser", subs);
    public async Task TriggerSubTestAsync() =>
        await _twitchAlertTypesService.HandleSubscriptionAsync("TestUser");

    
    // Commands Test
    public async Task TriggerCommandTestAsync(string command) =>
        await _commandAlertService.HandleCommandAsync(
            "!" + command, 
            "TestUser", 
            AppSettings.TWITCH_CHANNEL!,
            (channel, message) => _twitchClient.SendMessage(channel, message));

    // First Chatter Test
    public async Task TriggerFirstChatTestAsync(string username) =>
        await _firstChatterAlertService.HandleFirstChatAsync(username, username);

    public void TriggerFirstChatClear() =>
        _firstChatterAlertService.ClearFirstChatters();

    public async Task TriggerTextToSpeech(string message, string speaker) =>
        await _tsService.SpeakAsync(message, speaker, null);
}