namespace TK.Paddle
{
    public static class PaddleDomainErrorCodes
    {
        /* You can add your business exception error codes here, as constants */

        public const string PREFIX = "Paddle:";

        public const string UnexpectedException = PREFIX + "00000";

        public static string PaymentGeneratePayLinkFailed = PREFIX + "00001";
        public static string PaymentHasAnotherProcessing = PREFIX + "00002";
        public static string RequestCancelSubsciptionFailed = PREFIX + "00003";
    }
}
