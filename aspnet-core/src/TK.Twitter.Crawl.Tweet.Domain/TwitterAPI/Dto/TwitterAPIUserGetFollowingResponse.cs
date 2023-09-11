using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIUserGetFollowingResponse : TwitterAPIBaseResponse
    {
        [JsonProperty("data")]
        public DataDto Data { get; set; }

        public class DataDto
        {
            [JsonProperty("user")]
            public TwitterAPIUserDto User { get; set; }
        }
    }
}
