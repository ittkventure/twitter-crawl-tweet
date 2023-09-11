using Newtonsoft.Json;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIUserGetUserResponse: TwitterAPIBaseResponse
    {
        [JsonProperty("data")]
        public DataDto Data { get; set; }

        public class DataDto
        {
            [JsonProperty("users")]
            public List<TwitterAPIUserDto> Users { get; set; }
        }
    }
}
