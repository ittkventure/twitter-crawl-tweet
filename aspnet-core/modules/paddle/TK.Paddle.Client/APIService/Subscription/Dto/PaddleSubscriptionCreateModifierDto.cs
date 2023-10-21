using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Subscription.Dto
{
    public class PaddleSubscriptionCreateModifierDto
    {
        [JsonProperty("subscription_id")]
        public long SubscriptionId { get; set; }

        [JsonProperty("modifier_id")]
        public long ModifierId { get; set; }
    }
}
