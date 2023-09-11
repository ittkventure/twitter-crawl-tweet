using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterTweetSearchRecentDataDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("public_metrics")]
        public TwitterTweetPublicMetricDto PublicMetrics { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}