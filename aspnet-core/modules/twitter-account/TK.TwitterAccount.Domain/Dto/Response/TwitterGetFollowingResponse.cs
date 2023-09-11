using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterGetFollowingResponse
    {
        [JsonProperty("data")]
        public List<TwitterFollowingUserInfo> Data { get; set; }

        [JsonProperty("meta")]
        public TwitterMetaDto Meta { get; set; }

        [JsonProperty("errors")]
        public List<TwitterError> TwitterErrors { get; set; }
    }
}