public interface IAlertService
{
    void EnqueueAlert(string message, string? mediaPath = null);    
}