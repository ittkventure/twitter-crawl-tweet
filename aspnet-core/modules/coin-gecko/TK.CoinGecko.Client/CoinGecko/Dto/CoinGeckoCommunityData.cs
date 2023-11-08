using Newtonsoft.Json;

namespace TK.CoinGecko.Client.CoinGecko.Dto
{
    public class CoinGeckoCommunityData
    {
        [JsonProperty("facebook_likes")]
        public object FacebookLikes { get; set; }

        [JsonProperty("twitter_followers")]
        public int? TwitterFollowers { get; set; }

        [JsonProperty("reddit_average_posts_48h")]
        public double? RedditAveragePosts48h { get; set; }

        [JsonProperty("reddit_average_comments_48h")]
        public double? RedditAverageComments48h { get; set; }

        [JsonProperty("reddit_subscribers")]
        public int? RedditSubscribers { get; set; }

        [JsonProperty("reddit_accounts_active_48h")]
        public int? RedditAccountsActive48h { get; set; }

        [JsonProperty("telegram_channel_user_count")]
        public int? TelegramChannelUserCount { get; set; }
    }
}