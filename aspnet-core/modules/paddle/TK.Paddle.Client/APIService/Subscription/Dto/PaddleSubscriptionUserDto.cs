using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Subscription.Dto
{
    public class PaddleSubscriptionUserDto
    {
        [JsonProperty("subscription_id")]
        public long SubscriptionId { get; set; }

        [JsonProperty("plan_id")]
        public long PlanId { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("user_email")]
        public string UserEmail { get; set; }

        [JsonProperty("marketing_consent")]
        public bool MarketingConsent { get; set; }

        /// <summary>
        /// <see cref="PaddleConst.UserState"/>
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("signup_date")]
        public DateTime SignupDate { get; set; }

        [JsonProperty("last_payment")]
        public PaddleSubscriptionPaymentTimeDto LastPayment { get; set; }

        [JsonProperty("next_payment")]
        public PaddleSubscriptionPaymentTimeDto NextPayment { get; set; }

        [JsonProperty("update_url")]
        public string UpdateUrl { get; set; }

        [JsonProperty("cancel_url")]
        public string CancelUrl { get; set; }

        [JsonProperty("paused_at")]
        public DateTime PausedAt { get; set; }

        [JsonProperty("paused_from")]
        public DateTime PausedFrom { get; set; }

        [JsonProperty("payment_information")]
        public PaddleSubscriptionPaymentInformationDto PaymentInformation { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}
