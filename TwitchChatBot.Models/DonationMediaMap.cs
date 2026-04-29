using System.Text.Json.Serialization;

namespace TwitchChatBot.Models
{
	public class DonationMediaMap
	{
		[JsonPropertyName("default")]
		public string Default { get; set; } = "";

		[JsonPropertyName("tiers")]
		public List<DonationTier> Tiers { get; set; } = new();
	}
}