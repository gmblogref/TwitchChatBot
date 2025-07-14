public interface ITestUtilityService
{
    Task TriggerChannelPointTestAsync(string redemption);
    Task TriggerCheerTestAsync(int bits);
    void TriggerFollowTest(string userName);
    Task TriggerHypeTrainTestAsync();
    Task TriggerRaidTestAsync(string userName, int viewers);
    Task TriggerReSubTestAsync(string userName, int months);
    Task TriggerSubGiftTestAsync(string userName, string recipient);
    Task TriggerSubMysteryGiftTestAsync(string userName, int subs);
    Task TriggerSubTestAsync(string userName);

    Task TriggerCommandTestAsync(string command);

    Task TriggerFirstChatTestAsync(string username);
    void TriggerFirstChatClear();

    Task TriggerTextToSpeech(string message, string speaker);
}