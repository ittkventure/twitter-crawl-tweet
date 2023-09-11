using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterTweetCountHashTagDto
    {
        [JsonProperty("start")]
        public DateTime? Start { get; set; }

        [JsonProperty("end")]
        public DateTime? End { get; set; }

        [JsonProperty("tweet_count")]
        public int? TweetCount { get; set; }
    }
}