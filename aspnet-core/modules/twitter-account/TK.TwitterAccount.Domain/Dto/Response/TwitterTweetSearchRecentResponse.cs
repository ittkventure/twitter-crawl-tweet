using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterTweetSearchRecentResponse
    {
        [JsonProperty("data")]
        public List<TwitterTweetSearchRecentDataDto> Data { get; set; }

        [JsonProperty("meta")]
        public TwitterMetaDto Meta { get; set; }
    }


}