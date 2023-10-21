using Newtonsoft.Json;
using TK.Paddle.Client.APIService.Product.Dto;
using TK.Paddle.Client.Base;

namespace TK.Paddle.Client.APIService.Product.Response
{
    public class PaddleProductListProductsReponse : PaddleBaseResponse
    {
        [JsonProperty("response")]
        public PaddleProductProductListDto Response { get; set; }
    }
}
