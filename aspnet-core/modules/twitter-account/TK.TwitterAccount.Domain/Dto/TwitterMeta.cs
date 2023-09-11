using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterMetaDto
    {
        [JsonProperty("result_count")]
        public int ResultCount { get; set; }

        [JsonProperty("next_token")]
        public string NextToken { get; set; }

        [JsonProperty("newest_id")]
        public string NewestId { get; set; }

        [JsonProperty("oldest_id")]
        public string OldestId { get; set; }
    }
}