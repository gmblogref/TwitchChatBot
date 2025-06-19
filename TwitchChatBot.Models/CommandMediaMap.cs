namespace TwitchChatBot.Models
{
    public class CommandMediaMap
    {
        public List<CommandMediaItem> CommandMediaItems { get; set; } = new();
    }

    public class CommandMediaItem
    {
        public string Command { get; set; } = string.Empty;
        public string? Text { get; set; }
        public string? Media { get; set; }
    }

    /* PUT INTO JSON FILE WHEN READY
     * {
     * "commandMediaItems": [
     *   { "command": "!almost", "media": "commands/almostHadIt.mp3" },
     *   { "command": "!afraid", "media": "commands/Be_Afraid.mp3" },
     *   { "command": "!ads", "text": "Thank you for coming in to watch me. Yes Twitch ads are playing and I want to thank you for watching them to help support the stream. They will be over soon and then you can take your seat in the stadium to watch the game play. I do long ads so we have a bigger break between when the play." },
     *   { "command": "!babysacks", "media": "commands/babysacks.mp3" },
     *   { "command": "!brian", "media": "commands/balls.mp3" },
     *   { "command": "!chiefs", "media": "commands/FlagOnThePlay.mp3" },
     *   { "command": "!dedge", "media": "commands/Sonic_Game_Over_Sound_Effect.mp3" },
     *   { "command": "!discord", "text": "Sacks made a discord. Want to hang out for more content? Join here https://discord.gg/FKj7EJ6hJZ" },
     *   { "command": "!donoclip", "text": "Record a video or audio clip and have it played on stream. https://www.donoclip.com/legendofsacks" },
     *   { "command": "!excuses", "media": "commands/full_of_excuses.mp3" },
     *   { "command": "!fraid", "text": "HOME RUN RAID legend1111Raid legend1111Hey legend1111Showoff HOME RUN RAID legend1111Raid legend1111Hey legend1111Showoff HOME RUN RAID legend1111Raid legend1111Hey legend1111Showoff HOME RUN RAID legend1111Raid legend1111Hey legend1111Showoff" },
     *   { "command": "!gamer", "media": "commands/gamerAlert.mp3" },
     *   { "command": "!go", "media": "commands/Go_Ninja_Go.mp3" },
     *   { "command": "!gotham", "media": "commands/Gotham.mp3" },
     *   { "command": "!jilly", "media": "commands/sonic_extra_life.mp3", "text": "You got this! We believe in you!" },
     *   { "command": "!jinray", "media": "commands/JinRayLove.mp3", "text": "Hey its Hanna Mon Jin Tanner Ray Ross Troll Stern Bacca. Go check @jinray at https://www.twitch.tv/jinray" },
     *   { "command": "!justice", "media": "commands/doggyTreat.mp3" },
     *   { "command": "!kc", "media": "commands/KC.mp3" },
     *   { "command": "!lie", "media": "commands/ThatIsALie.mp3" },
     *   { "command": "!meow", "media": "commands/Meow.mp3" },
     *   { "command": "!move", "media": "commands/MoveIt_MoveIt.mp3" },
     *   { "command": "!phrasing", "media": "commands/phrasing.mp3" },
     *   { "command": "!puppys", "media": "commands/ForThePuppies.mp3" },
     *   { "command": "!ref", "media": "commands/FlagOnThePlay.mp3" },
     *   { "command": "!relax", "media": "commands/WhoaRelax.mp3" },
     *   { "command": "!sacks", "media": "commands/LegendOfSacks.mp3" },
     *   { "command": "!sackscrawl", "media": "commands/BitesTheDust.mp3" },
     *   { "command": "!sacksfail", "media": "commands/fail.mp3", "text": "See, the problem is you thought that Sacks could succeed" },
     *   { "command": "!sacksrocks", "media": "commands/party_like_a_rockstar.mp3" },
     *   { "command": "!sackswin", "media": "commands/SacksWin.mp3" },
     *   { "command": "!scherbie", "media": "commands/Scherbie_Love.mp3" },
     *   { "command": "!screen", "media": "commands/GameScreen.mp3" },
     *   { "command": "!so", "text": "If you enjoy my stuff please go check out and follow @$targetname at -- $url  -- They were last seen playing  $game" },
     *   { "command": "!sraid", "text": "GRAND SLAM RAID legend1111Raid legend1111Pog legend1111HR legend1111Salute GRAND SLAM RAID legend1111Raid legend1111Pog legend1111HR legend1111Salute GRAND SLAM RAID legend1111Raid legend1111Pog legend1111HR legend1111Salute GRAND SLAM RAID legend1111Raid legend1111Pog legend1111HR legend1111Salute" },
     *   { "command": "!spikes", "media": "commands/SpikesAreBad.mp3" },
     *   { "command": "!squeak", "media": "commands/squeek.mp3" },
     *   { "command": "!unprepared", "media": "commands/ISH Together.mp3" },
     *   { "command": "!whipped", "media": "commands/whipped.mp3" },
     *   { "command": "!winded", "media": "commands/windblowing.mp3" }
     * ]
     * }
     */
}
