using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Subscription.Dto
{
    public class PaddleSubscriptionPaymentDto
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("subscription_id")]
        public string SubscriptionId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("payout_date")]
        public DateTime PayoutDate { get; set; }

        [JsonProperty("is_paid")]
        public int IsPaid { get; set; }

        [JsonProperty("is_one_off_charge")]
        public bool IsOneOffCharge { get; set; }

        [JsonProperty("receipt_url")]
        public string ReceiptUrl { get; set; }
    }
}
