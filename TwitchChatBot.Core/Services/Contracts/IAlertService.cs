using TwitchChatBot.Models;

public interface IAlertService
{
    void EnqueueAlert(string message, string? mediaPath = null);

	void EnqueueAlert(AlertItem alert);
}