using Newtonsoft.Json;
using TK.Paddle.Client.APIService.Subscription.Dto;
using TK.Paddle.Client.Base;

namespace TK.Paddle.Client.APIService.Subscription.Response
{
    public class PaddleSubscriptionCreateModifierResponse : PaddleBaseResponse
    {
        [JsonProperty("response")]
        public PaddleSubscriptionCreateModifierDto Response { get; set; }
    }
}
