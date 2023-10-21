using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Subscription.Dto
{
    public class PaddleSubscriptionUpdateUserDto
    {
        [JsonProperty("subscription_id")]
        public long SubscriptionId { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("plan_id")]
        public long PlanId { get; set; }

        [JsonProperty("next_payment")]
        public PaddleSubscriptionPaymentTimeDto NextPayment { get; set; }
    }
}
