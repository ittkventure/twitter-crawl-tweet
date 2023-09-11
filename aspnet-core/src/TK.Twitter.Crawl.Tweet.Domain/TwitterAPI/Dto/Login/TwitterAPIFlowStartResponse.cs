using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto.Login
{
    public class TwitterAPIFlowStartResponse
    {
        [JsonProperty("flow_token")]
        public string FlowToken { get; set; }
    }
}
