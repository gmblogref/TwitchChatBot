using TwitchChatBot.Models;

public interface IAlertService
{
    void EnqueueAlert(string message, string? mediaPath = null);

	void EnqueueAlert(AlertItem alert);
	Task<bool> TryCancelActiveTtsAsync();
	Task<IReadOnlyList<string>> CancelTtsAsync(long ttsSequence);
	Task<IReadOnlyList<string>> ResetTtsAsync(long throughTtsSequence);
}
