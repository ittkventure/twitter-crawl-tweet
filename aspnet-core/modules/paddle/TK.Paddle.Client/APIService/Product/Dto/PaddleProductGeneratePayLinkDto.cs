using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Product.Dto
{
    public class PaddleProductGeneratePayLinkDto
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

}
