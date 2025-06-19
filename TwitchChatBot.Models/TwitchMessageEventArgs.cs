public class TwitchMessageEventArgs : EventArgs
{
    public string Channel { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}