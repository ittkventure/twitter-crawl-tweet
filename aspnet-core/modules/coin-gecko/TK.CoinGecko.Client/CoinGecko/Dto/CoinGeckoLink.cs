using Newtonsoft.Json;

namespace TK.CoinGecko.Client.CoinGecko.Dto
{
    public class CoinGeckoLink
    {
        [JsonProperty("homepage")]
        public List<string> Homepage { get; set; }

        [JsonProperty("blockchain_site")]
        public List<string> BlockchainSite { get; set; }

        [JsonProperty("official_forum_url")]
        public List<string> OfficialForumUrl { get; set; }

        [JsonProperty("chat_url")]
        public List<string> ChatUrl { get; set; }

        [JsonProperty("announcement_url")]
        public List<string> AnnouncementUrl { get; set; }

        [JsonProperty("twitter_screen_name")]
        public string TwitterScreenName { get; set; }

        [JsonProperty("facebook_username")]
        public string FacebookUsername { get; set; }

        [JsonProperty("bitcointalk_thread_identifier")]
        public object BitcointalkThreadIdentifier { get; set; }

        [JsonProperty("telegram_channel_identifier")]
        public string TelegramChannelIdentifier { get; set; }

        [JsonProperty("subreddit_url")]
        public object SubredditUrl { get; set; }

        [JsonProperty("repos_url")]
        public CoinGeckoRepoUrl ReposUrl { get; set; }
    }
}