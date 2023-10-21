using Newtonsoft.Json;
using TK.Paddle.Client.APIService.Subscription.Dto;
using TK.Paddle.Client.Base;

namespace TK.Paddle.Client.APIService.Subscription.Response
{
    public class PaddleSubscriptionUpdatePostcodeResponse : PaddleBaseResponse
    {
        [JsonProperty("response")]
        public PaddleSubscriptionUpdatePostcodeDto Response { get; set; }
    }
}
