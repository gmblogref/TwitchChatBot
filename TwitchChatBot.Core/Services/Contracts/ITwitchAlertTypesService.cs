public interface ITwitchAlertTypesService
{
    Task HandleChannelPointRedemptionAsync(string username, string rewardTitle);
    Task HandleCheerAsync(string username, int bits, string message);
    Task HandleFollowAsync(string username);
    Task HandleHypeTrainAsync();
    Task HandleRaidAsync(string username, int viewers);
    Task HandleResubAsync(string username, int months, string userMessage);
    Task HandleSubGiftAsync(string username, string recipient);
    Task HandleSubMysteryGiftAsync(string username, int numOfSubs);
    Task HandleSubscriptionAsync(string username);
}