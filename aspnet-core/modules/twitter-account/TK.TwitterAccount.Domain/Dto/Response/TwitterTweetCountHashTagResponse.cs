using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterTweetCountHashTagResponse
    {
        [JsonProperty("data")]
        public List<TwitterTweetCountHashTagDto> Data { get; set; }

        [JsonProperty("meta")]
        public TwitterTweetCountMetaDto Meta { get; set; }
    }
}