using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIClientEventInfoDto
    {
        [JsonProperty("component")]
        public string Component { get; set; }

        [JsonProperty("element")]
        public string Element { get; set; }
    }


}
