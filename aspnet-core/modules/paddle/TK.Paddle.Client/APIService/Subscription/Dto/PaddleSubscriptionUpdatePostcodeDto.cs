using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Subscription.Dto
{
    public class PaddleSubscriptionUpdatePostcodeDto
    {
        [JsonProperty("subscription_id")]
        public long SubscriptionId { get; set; }

        [JsonProperty("postcode")]
        public string postcode { get; set; }
    }
}
