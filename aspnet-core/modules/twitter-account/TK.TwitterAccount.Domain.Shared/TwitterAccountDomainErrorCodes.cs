namespace TK.TwitterAccount
{
    public static class TwitterAccountDomainErrorCodes
    {
        /* You can add your business exception error codes here, as constants */

        public const string PREFIX = "TwitterAccount:";

        public const string UnexpectedException = PREFIX + "00000";
        public const string Unauthorized = PREFIX + "00001";
        public const string ForbidenResource = PREFIX + "00002";
        public const string NotFound = PREFIX + "00003";
        public const string TooManyRequest = PREFIX + "00004";

    }
}

