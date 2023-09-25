using Newtonsoft.Json;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIUserGetUserResponse : TwitterAPIBaseResponse
    {
        public string JsonContent { get; set; }
    }
}
