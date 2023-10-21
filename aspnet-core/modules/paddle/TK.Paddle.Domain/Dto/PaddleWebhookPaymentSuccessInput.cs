using System;

namespace TK.Paddle.Domain.Dto
{
    public class PaddleWebhookSubscriptionPaymentSuccessInput : PaddleWebhookBaseInput
    {
        public string BalanceCurrency { get; set; }

        public decimal? BalanceEarnings { get; set; }

        public decimal? BalanceFee { get; set; }

        public decimal? BalanceGross { get; set; }

        public decimal? BalanceTax { get; set; }

        public string CheckoutId { get; set; }

        public string Country { get; set; }

        public string Coupon { get; set; }

        public string Currency { get; set; }

        public string CustomData { get; set; }

        public string CustomerName { get; set; }

        public decimal? Earnings { get; set; }

        public string Email { get; set; }

        public DateTime? EventTime { get; set; }

        public decimal? Fee { get; set; }

        public string Ip { get; set; }

        public string MarketingConsent { get; set; }

        public string OrderId { get; set; }

        public string Passthrough { get; set; }

        public string PaymentMethod { get; set; }

        public decimal? PaymentTax { get; set; }

        public long? ProductId { get; set; }

        public string ProductName { get; set; }

        public int? Quantity { get; set; }

        public string ReceiptUrl { get; set; }

        public decimal? SaleGross { get; set; }

        public decimal? UsedPriceOverride { get; set; }

        public decimal? InitialPayment { get; set; }

        public decimal Instalments { get; set; }

        public DateTime NextBillDate { get; set; }

        public decimal? NextPaymentAmount { get; set; }

        public string PlanName { get; set; }

        public string Status { get; set; }

        public long SubscriptionId { get; set; }

        public long SubscriptionPaymentId { get; set; }

        public long SubscriptionPlanId { get; set; }

        public string UnitPrice { get; set; }

        public string UserId { get; set; }
    }
}