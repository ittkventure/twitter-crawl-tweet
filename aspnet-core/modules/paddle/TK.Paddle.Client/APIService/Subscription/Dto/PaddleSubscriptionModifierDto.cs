using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Subscription.Dto
{
    public class PaddleSubscriptionModifierDto
    {
        [JsonProperty("modifier_id")]
        public long ModifierId { get; set; }

        [JsonProperty("subscription_id")]
        public long SubscriptionId { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("is_recurring")]
        public bool IsRecurring { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
