using Newtonsoft.Json;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPITimelineDto
    {
        [JsonProperty("timeline")]
        public TwitterAPITimelineDto Timeline { get; set; }

        [JsonProperty("instructions")]
        public List<TwitterAPIInstructionDto> Instructions { get; set; }
    }


}
