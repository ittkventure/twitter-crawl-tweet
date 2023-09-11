using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIContentDto
    {
        [JsonProperty("entryType")]
        public string EntryType { get; set; }

        [JsonProperty("__typename")]
        public string Typename { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("itemContent")]
        public TwitterAPIItemContentDto ItemContent { get; set; }

        [JsonProperty("clientEventInfo")]
        public TwitterAPIClientEventInfoDto ClientEventInfo { get; set; }
    }


}
