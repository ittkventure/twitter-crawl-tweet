using Newtonsoft.Json;

namespace TK.CoinGecko.Client.CoinGecko.Dto
{
    public class CoinGeckoImage
    {
        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }
    }
}