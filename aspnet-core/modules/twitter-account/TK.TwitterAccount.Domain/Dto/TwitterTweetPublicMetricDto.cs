using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterTweetPublicMetricDto
    {
        [JsonProperty("retweet_count")]
        public int? RetweetCount { get; set; }

        [JsonProperty("reply_count")]
        public int? ReplyCount { get; set; }

        [JsonProperty("like_count")]
        public int? LikeCount { get; set; }

        [JsonProperty("quote_count")]
        public int? QuoteCount { get; set; }
    }
}