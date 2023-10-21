using Newtonsoft.Json;
using TK.Paddle.Client.APIService.Subscription.Dto;
using TK.Paddle.Client.Base;

namespace TK.Paddle.Client.APIService.Subscription.Response
{
    public class PaddleSubscriptionCreateOneOffChargeResponse : PaddleBaseResponse
    {
        [JsonProperty("response")]
        public PaddleSubscriptionCreateOneOffChargeDto Response { get; set; }
    }
}
