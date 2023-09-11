using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIItemContentDto
    {
        [JsonProperty("itemType")]
        public string ItemType { get; set; }

        [JsonProperty("__typename")]
        public string Typename { get; set; }

        [JsonProperty("user_results")]
        public TwitterAPIUserResultsDto UserResults { get; set; }

        [JsonProperty("userDisplayType")]
        public string UserDisplayType { get; set; }
    }


}
