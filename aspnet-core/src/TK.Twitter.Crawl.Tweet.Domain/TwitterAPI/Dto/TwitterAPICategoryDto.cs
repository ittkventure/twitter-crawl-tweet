using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPICategoryDto
    {
        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("icon_name")]
        public string IconName { get; set; }
    }


}
