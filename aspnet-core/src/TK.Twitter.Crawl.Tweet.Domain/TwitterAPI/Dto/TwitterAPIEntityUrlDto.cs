using Newtonsoft.Json;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIEntityUrlDto
    {
        [JsonProperty("urls")]
        public List<TwitterAPIUrlDto> Urls { get; set; }
    }
}
