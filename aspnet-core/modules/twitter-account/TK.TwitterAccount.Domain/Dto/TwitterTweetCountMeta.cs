using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterTweetCountMetaDto
    {
        [JsonProperty("total_tweet_count")]
        public int TotalTweetCount { get; set; }
    }
}