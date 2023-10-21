using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Product.Dto
{
    public class PaddleProductProductListDto
    {
        [JsonProperty("total")]
        public string Total { get; set; }

        [JsonProperty("count")]
        public string Count { get; set; }

        [JsonProperty("products")]
        public List<PaddleProductProductDto> Products { get; set; }
    }

}
