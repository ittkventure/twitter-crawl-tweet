using Newtonsoft.Json;

namespace TK.Twitter.Crawl.TwitterAPI.Dto.Login
{
    public class TwitterAPIActivateResponse
    {
        [JsonProperty("guest_token")]
        public string GuestToken { get; set; }
    }
}
