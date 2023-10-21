using Newtonsoft.Json;

namespace TK.Paddle.Client.APIService.Subscription.Dto
{
    public class PaddleSubscriptionPaymentInformationDto
    {
        /// <summary>
        /// <see cref="PaddleConst.PaymentMethod"/>
        /// </summary>
        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// <see cref="PaddleConst.CardType"/>
        /// </summary>
        [JsonProperty("card_type")]
        public string CardType { get; set; }

        [JsonProperty("last_four_digits")]
        public string LastFourDigits { get; set; }

        [JsonProperty("expiry_date")]
        public DateTime ExpiryDate { get; set; }
    }
}
