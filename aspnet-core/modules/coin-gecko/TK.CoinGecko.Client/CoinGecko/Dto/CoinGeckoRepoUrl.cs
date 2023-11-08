using Newtonsoft.Json;

namespace TK.CoinGecko.Client.CoinGecko.Dto
{
    public class CoinGeckoRepoUrl
    {
        [JsonProperty("github")]
        public List<string> Github { get; set; }

        [JsonProperty("bitbucket")]
        public List<object> Bitbucket { get; set; }
    }
}