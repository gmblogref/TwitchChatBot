using System.Drawing;

public class TwitchMessageEventArgs : EventArgs
{
    public string Channel { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Color Color { get; set; } = Color.White;
}