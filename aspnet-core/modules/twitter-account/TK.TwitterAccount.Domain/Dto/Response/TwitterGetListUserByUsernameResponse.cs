using Newtonsoft.Json;

namespace TK.TwitterAccount.Domain.Dto
{
    public class TwitterGetListUserByUsernameResponse
    {
        [JsonProperty("data")]
        public List<TwitterGetListUserByUsernameDto> Data { get; set; }

        [JsonProperty("errors")]
        public List<TwitterError> Errors { get; set; }
    }


}