using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatBot.Models
{
	public class GiftSubBundleSuppressionState
	{
		public string GifterKey { get; set; } = string.Empty;

		public int RemainingGiftCount { get; set; }

		public DateTime ExpiresAtUtc { get; set; }
	}
}
