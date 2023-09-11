using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIUserResultsDto
    {
        [JsonProperty("result")]
        public TwitterAPIResultDto Result { get; set; }
    }

}
