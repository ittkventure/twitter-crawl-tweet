using Newtonsoft.Json;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIInstructionDto
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }

        [JsonProperty("entries")]
        public List<TwitterAPIEntryDto> Entries { get; set; }
    }


}
