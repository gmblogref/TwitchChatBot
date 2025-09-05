public interface IModerationService
{
    Task<string> GetUserIdAsync(string login, CancellationToken ct = default);
    Task TimeoutAsync(string broadcasterId, string moderatorId, string targetUserId, int seconds, CancellationToken ct = default);
}
