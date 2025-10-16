public interface ITwitchAlertTypesService
{
    Task HandleChannelPointRedemptionAsync(string username, string rewardTitle);
    Task HandleCheerAsync(string username, int bits, string message);
    Task HandleFollowAsync(string username);
    Task HandleHypeTrainAsync();
    Task HandleRaidAsync(string username, int viewers);
    Task HandleResubAsync(string username, int months, string userMessage, string subTier);
    Task HandleSubGiftAsync(string username, string recipient, string subTier);
    Task HandleSubMysteryGiftAsync(string username, int numOfSubs, string subTier);
    Task HandleSubscriptionAsync(string username, string subTier);
    Task HandleWatchStreakNoticeAsync(string username, int streakCount, string? userMessage);
}