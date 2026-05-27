namespace TwitchChatBot.Core.Services.Contracts
{
	public interface IGiftSubBundleSuppressionService
	{
		void TrackBundle(string gifterKey, int expectedGiftCount);

		bool ShouldSuppressIndividualGift(string gifterKey);
	}
}