public interface IChattersService
{
    Task<List<string>> GetCurrentChattersAsync(string channelName);
}