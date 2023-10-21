using Newtonsoft.Json;
using TK.Paddle.Client.APIService.Product.Dto;
using TK.Paddle.Client.Base;

namespace TK.Paddle.Client.APIService.Product.Response
{
    public class PaddleProductGeneratePayLinkReponse : PaddleBaseResponse
    {
        [JsonProperty("response")]
        public PaddleProductGeneratePayLinkDto Response { get; set; }
    }
}
