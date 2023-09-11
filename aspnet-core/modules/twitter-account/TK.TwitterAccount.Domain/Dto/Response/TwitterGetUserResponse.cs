using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterGetUserResponse
    {
        [JsonProperty("data")]
        public TwitterUserDto Data { get; set; }

        [JsonProperty("meta")]
        public TwitterMetaDto Meta { get; set; }
    }
}