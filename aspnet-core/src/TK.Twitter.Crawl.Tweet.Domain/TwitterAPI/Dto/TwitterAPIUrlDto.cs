using Newtonsoft.Json;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIUrlDto
    {
        [JsonProperty("display_url")]
        public string DisplayUrl { get; set; }

        [JsonProperty("expanded_url")]
        public string ExpandedUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("indices")]
        public List<int?> Indices { get; set; }
    }
}
