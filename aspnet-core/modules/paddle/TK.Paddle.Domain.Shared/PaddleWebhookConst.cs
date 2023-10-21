namespace TK.Paddle.Domain.Shared
{
    public class PaddleWebhookConst
    {
        public static class AlertName
        {
            public const string PAYMENT_SUCCEEDED = "payment_succeeded";

            public const string SUBSCRIPTION_PAYMENT_SUCCEEDED = "subscription_payment_succeeded";
            public const string SUBSCRIPTION_CANCELLED = "subscription_cancelled";
        }
    }
}