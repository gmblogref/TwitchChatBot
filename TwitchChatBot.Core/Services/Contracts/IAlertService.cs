public interface IAlertService
{
    void EnqueueAlert(string message, string? mediaPath = null);
    void HandleChannelPointAlert(string rewardTitle, string username);
    void HandleCheerAlert(string username, int bits, string chatMessage);
    void HandleFollowAlert(string username);
    void HandleGiftSubAlert(string gifter, string recipient);
    void HandleHypeTrainAlert();
    void HandleMysteryGiftAlert(string username, int subCount);
    void HandleRaidAlert(string username, int viewers);
    void HandleResubAlert(string username, int months, string resubMessage);
    void HandleSubAlert(string username);
    void StartAdTimer(TimeSpan interval);
    void StopAdTimer();
}
