using Newtonsoft.Json;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.TwitterAPI.Dto.Login
{
    public class TwitterAPIFlowResponse
    {
        [JsonProperty("flow_token")]
        public string FlowToken { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        public IEnumerable<string> Cookie { get; set; }
    }
}
