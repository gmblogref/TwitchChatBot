public interface ITwitchAlertTypesService
{
    Task HandleChannelPointRedemptionAsync(string username, string rewardTitle, IAlertService alertService); 
    Task HandleCheerAsync(string username, int bits, string message, IAlertService alertService);
    Task HandleHypeTrainAsync(IAlertService alertService);
    Task HandleRaidAsync(string username, int viewers, IAlertService alertService);
    Task HandleResubAsync(string username, int months, string userMessage, IAlertService alertService);
    Task HandleSubGiftAsync(string username, string recipient, IAlertService alertService);
    Task HandleSubMysteryGiftAsync(string username, int numOfSubs, IAlertService alertService);
    Task HandleSubscriptionAsync(string username, IAlertService alertService);
}