using System;

namespace TK.Paddle.Domain.Dto
{
    public class PaddleWebhookSubscriptionCanceledInput : PaddleWebhookBaseInput
    {
        public DateTime? CancellationEffectiveDate { get; set; }

        public string CheckoutId { get; set; }

        public string Coupon { get; set; }

        public string Currency { get; set; }

        public string CustomData { get; set; }

        public string Email { get; set; }

        public DateTime? EventTime { get; set; }

        public string Passthrough { get; set; }

        public int? Quantity { get; set; }

        public string Status { get; set; }

        public long SubscriptionId { get; set; }

        public long SubscriptionPlanId { get; set; }

        public string UnitPrice { get; set; }

        public string UserId { get; set; }
    }
}