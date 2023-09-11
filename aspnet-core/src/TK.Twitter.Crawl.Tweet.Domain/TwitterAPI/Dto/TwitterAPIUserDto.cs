using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIUserDto
    {
        [JsonProperty("result")]
        public TwitterAPIResultDto Result { get; set; }
    }


}
