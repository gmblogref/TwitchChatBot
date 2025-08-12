namespace TwitchChatBot.Core.Constants
{
    public static class TwitchEventTypes
    {
        public const string ChannelSubscribe = "channel.subscribe";
        public const string ChannelSubscriptionGift = "channel.subscription.gift";
        public const string ChannelSubscriptionMessage = "channel.subscription.message";
        public const string ChannelCheer = "channel.cheer";
        public const string ChannelRaid = "channel.raid";

        public const string ChannelPointsRedemption = "channel.channel_points_custom_reward_redemption.add";
        public const string ChannelPointsRedemptionUpdate = "channel.channel_points_custom_reward_redemption.update";

        public const string HypeTrainBegin = "channel.hype_train.begin";
        public const string HypeTrainProgress = "channel.hype_train.progress";
        public const string HypeTrainEnd = "channel.hype_train.end";

        public const string ChannelFollow = "channel.follow";
    }
}