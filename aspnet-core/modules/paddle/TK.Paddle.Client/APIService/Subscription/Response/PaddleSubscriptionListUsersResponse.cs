using Newtonsoft.Json;
using TK.Paddle.Client.APIService.Subscription.Dto;
using TK.Paddle.Client.Base;

namespace TK.Paddle.Client.APIService.Subscription.Response
{
    public class PaddleSubscriptionListUsersResponse : PaddleBaseResponse
    {
        [JsonProperty("response")]
        public List<PaddleSubscriptionUserDto> Response { get; set; }
    }
}
