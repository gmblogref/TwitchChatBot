public interface ITestUtilityService
{
    Task TriggerChannelPointTestAsync(string redemption);
    Task TriggerCheerTestAsync(int bits);
    void TriggerFollowTest();
    Task TriggerHypeTrainTestAsync();
    Task TriggerRaidTestAsync();
    Task TriggerReSubTestAsync();
    Task TriggerSubGiftTestAsync();
    Task TriggerSubMysteryGiftTestAsync(int subs);
    Task TriggerSubTestAsync();

    Task TriggerCommandTestAsync(string command);

    Task TriggerFirstChatTestAsync(string username);
    void TriggerFirstChatClear();

    Task TriggerTextToSpeech(string message, string speaker);
}