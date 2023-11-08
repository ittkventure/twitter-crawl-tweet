namespace TK.CoinGecko.Client.CoinGecko
{
    public static class CoinGeckoQaCode
    {
        public const string PREFIX = "CoinGecko:";

        /// <summary>
        /// This Exception was throw when the bussiness execute facing crash
        /// </summary>
        public static string UnExpectedException = PREFIX + "10400.500";

        public static string TooManyRequestError = PREFIX + "10500.429";

        public static string NotFound = PREFIX + "10500.404";
    }
}