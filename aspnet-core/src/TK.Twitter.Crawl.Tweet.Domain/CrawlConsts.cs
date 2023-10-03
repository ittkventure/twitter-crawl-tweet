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

    public static class Signal
    {
        public const string LISTING_CEX = "LISTING_CEX";
        public const string SPONSORED_TWEETS = "SPONSORED_TWEETS";
        public const string JUST_AUDITED = "JUST_AUDITED";
        public const string UPCOMMING_TOKEN_SALE = "UPCOMMING_TOKEN_SALE";
        public const string HOSTING_GIVEAWAYS = "HOSTING_GIVEAWAYS";

        public static string GetName(string signal)
        {
            string name;
            switch (signal)
            {
                case LISTING_CEX:
                    name = "Listing cex";
                    break;
                case JUST_AUDITED:
                    name = "Audit is completed";
                    break;
                case SPONSORED_TWEETS:
                    name = "Buying sponsored ads";
                    break;
                case UPCOMMING_TOKEN_SALE:
                    name = "Upcomming token sales";
                    break;
                case HOSTING_GIVEAWAYS:
                    name = "Hosting Giveaways";
                    break;
                default:
                    name = null;
                    break;
            }

            return name;
        }

    }
}
