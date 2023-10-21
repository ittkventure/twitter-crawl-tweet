using Newtonsoft.Json;
using TK.Paddle.Client.APIService.Subscription.Dto;
using TK.Paddle.Client.Base;

namespace TK.Paddle.Client.APIService.Subscription.Response
{
    public class PaddleSubscriptionListPlansResponse : PaddleBaseResponse
    {
        [JsonProperty("response")]
        public List<PaddleSubscriptionPlanDto> Response { get; set; }
    }
}
