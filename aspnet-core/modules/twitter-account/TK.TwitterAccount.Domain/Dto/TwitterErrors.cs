using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterError
    {
        [JsonProperty("parameter")]
        public string Parameter { get; set; }

        [JsonProperty("resource_id")]
        public string ResourceId { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("resource_type")]
        public string ResourceType { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}