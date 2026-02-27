namespace TwitchChatBot.Models
{
    public class HypeTrainEnd
    {
        public int Level { get; set; } = 0;
        public string? TopCheerUser { get; set; }
        public int TopCheerBits { get; set; } = 0;
        public string? TopGiftsubUser { get; set; }
        public int TopGiftsubs { get; set; } = 0;
    }
}