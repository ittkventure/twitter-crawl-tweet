namespace TK.Paddle.Domain.Shared
{
    public static class PaddleConst
    {
        public static class UserState
        {
            public const string ACTIVE = "active";
            public const string PAST_DUE = "past_due";
            public const string TRIALING = "trialing";
            public const string PAUSED = "paused";
            public const string DELETED = "deleted";
        }

        public static class PaymentMethod
        {
            public const string PAYPAL = "paypal";
            public const string CARD = "card";
        }

        public static class CardType
        {
            public const string AMERICAN_EXPRESS = "american_express";
            public const string DINERS_CLUB = "diners_club";
            public const string DISCOVER = "discover";
            public const string JCB = "jcb";
            public const string MADA = "mada";
            public const string MAESTRO = "maestro";
            public const string MASTER = "master";
            public const string UNION_PAY = "union_pay";
            public const string VISA = "visa";
            public const string UNKNOWN = "unknown";
        }

        public static class CreateOneOffChargeStatus
        {
            public const string SUCCESS = "success";
            public const string PENDING = "pending";
        }

        public static class SubscriptionStatus
        {
            public const string ACTIVE = "active";
            public const string TRIALING = "trialing";
            public const string PAST_DUE = "past_due";
            public const string PAUSED = "paused";
            public const string DELETED = "deleted";
        }
    }
}
