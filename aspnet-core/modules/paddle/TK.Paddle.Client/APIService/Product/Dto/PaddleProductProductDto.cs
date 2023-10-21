using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Product.Dto
{
    public class PaddleProductProductDto
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("base_price")]
        public decimal? BasePrice { get; set; }

        [JsonProperty("sale_price")]
        public object SalePrice { get; set; }

        [JsonProperty("screenshots")]
        public List<object> Screenshots { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

}
