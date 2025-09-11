public interface IAlertService
{
    void EnqueueAlert(string message, string? mediaPath = null);
    void EnqueueAlert(string type, string message, string? mediaPath = null);
}