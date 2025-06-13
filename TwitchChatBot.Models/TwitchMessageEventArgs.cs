public class TwitchMessageEventArgs : EventArgs
{
    public string? Channel { get; set; }
    public string? Username { get; set; }
    public string? Message { get; set; }
}