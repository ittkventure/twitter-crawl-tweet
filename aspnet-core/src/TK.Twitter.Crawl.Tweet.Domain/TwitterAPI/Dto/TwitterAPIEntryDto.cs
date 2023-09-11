using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIEntryDto
    {
        [JsonProperty("entryId")]
        public string EntryId { get; set; }

        [JsonProperty("sortIndex")]
        public string SortIndex { get; set; }

        [JsonProperty("content")]
        public TwitterAPIContentDto Content { get; set; }
    }


}
