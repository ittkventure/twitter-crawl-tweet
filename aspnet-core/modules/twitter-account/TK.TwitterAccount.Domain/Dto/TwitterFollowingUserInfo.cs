using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterFollowingUserInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("public_metrics")]
        public TwitterPublicMetricsDto PublicMetrics { get; set; }
    }
}