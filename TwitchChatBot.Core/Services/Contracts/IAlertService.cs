public interface IAlertService
{
    void EnqueueAlert(string message, string? mediaPath = null);
    void StartAdTimer(TimeSpan interval);
    void StopAdTimer();
}
