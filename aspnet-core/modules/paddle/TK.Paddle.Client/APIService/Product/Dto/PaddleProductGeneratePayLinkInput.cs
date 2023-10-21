using System.Text.Json.Serialization;
using TK.Paddle.Client.Base.Enum;

namespace TK.Paddle.Client.APIService.Product.Dto
{
    public class PaddleProductGeneratePayLinkInput
    {
        public long? ProductId { get; set; }

        public string Title { get; set; }

        public string WebhookUrl { get; set; }

        public List<string> Prices { get; set; }

        public List<string> RecurringPrices { get; set; }

        public int? TrialDays { get; set; }

        public string CustomMessage { get; set; }

        public string CouponCode { get; set; }

        public PaddleYesNoEnum? Discountable { get; set; }

        public string ImageUrl { get; set; }

        public string ReturnUrl { get; set; }

        public PaddleYesNoEnum? QuantityVariable { get; set; }

        public int? Quantity { get; set; }

        public string Expires { get; set; }

        public PaddleYesNoEnum? MarketingConsent { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerCountry { get; set; }

        public string CustomerPostcode { get; set; }

        public PaddleYesNoEnum? IsRecoverable { get; set; }

        public string Passthrough { get; set; }

        public string VatNumber { get; set; }

        public string VatCompanyName { get; set; }

        public string VatStreet { get; set; }

        public string VatCity { get; set; }

        public string VatState { get; set; }

        public string VatCountry { get; set; }

        public string VatPostcode { get; set; }
    }

}
