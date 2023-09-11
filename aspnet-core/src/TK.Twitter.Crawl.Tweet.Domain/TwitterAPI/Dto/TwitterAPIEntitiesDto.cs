using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIEntitiesDto
    {
        [JsonProperty("description")]
        public TwitterAPIDescriptionDto Description { get; set; }

        [JsonProperty("url")]
        public TwitterAPIEntityUrlDto Url { get; set; }
    }
}
