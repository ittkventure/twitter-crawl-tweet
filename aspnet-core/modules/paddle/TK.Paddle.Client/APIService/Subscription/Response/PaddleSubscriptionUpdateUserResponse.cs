using Newtonsoft.Json;
using TK.Paddle.Client.APIService.Subscription.Dto;
using TK.Paddle.Client.Base;

namespace TK.Paddle.Client.APIService.Subscription.Response
{
    public class PaddleSubscriptionUpdateUserResponse : PaddleBaseResponse
    {
        [JsonProperty("response")]
        public PaddleSubscriptionUpdateUserDto Response { get; set; }
    }
}
