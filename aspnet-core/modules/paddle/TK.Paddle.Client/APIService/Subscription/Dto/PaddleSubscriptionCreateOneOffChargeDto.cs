using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Subscription.Dto
{
    public class PaddleSubscriptionCreateOneOffChargeDto
    {
        [JsonProperty("invoice_id")]
        public long InvoiceId { get; set; }

        [JsonProperty("subscription_id")]
        public long SubscriptionId { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("payment_date")]
        public string PaymentDate { get; set; }

        [JsonProperty("receipt_url")]
        public string ReceiptUrl { get; set; }

        /// <summary>
        /// <see cref="PaddleConst.CreateOneOffChargeStatus"/>
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

    }
}
