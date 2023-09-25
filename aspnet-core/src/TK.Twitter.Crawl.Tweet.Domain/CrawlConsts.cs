using System.Collections.Generic;

namespace TK.Twitter.Crawl;

public static class CrawlConsts
{
    public const string DbTablePrefix = "App";

    public const string DbSchema = null;

    public static class LeadType
    {
        public const string LEADS = "LEADS";
        public const string RETAILS = "RETAILS";
        public const string BIG_PARTNERS = "BIG_PARTNERS";
        public const string MEDIA = "MEDIA";
        public const string CEX = "CEX";
        public const string NON_CRYPTO_LEADS = "non-crypto leads";
        public const string Launchpad = "Launchpad";

        public static readonly List<string> AllowList = new()
        {
            LEADS,
            RETAILS,
            BIG_PARTNERS,
            MEDIA,
            CEX,
            NON_CRYPTO_LEADS,
            Launchpad
        };

    }
}
