using Newtonsoft.Json;
using TK.Paddle.Client.APIService.Subscription.Dto;
using TK.Paddle.Client.Base;

namespace TK.Paddle.Client.APIService.Subscription.Response
{
    public class PaddleSubscriptionListPaymentsResponse : PaddleBaseResponse
    {
        [JsonProperty("response")]
        public List<PaddleSubscriptionPaymentDto> Response { get; set; }
    }
}
