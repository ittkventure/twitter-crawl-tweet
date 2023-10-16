using System;
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
        public class Source
        {
            public const string TWITTER_TWEET = "TWITTER_TWEET";
            public const string AIR_TABLE_MANUAL_SOURCE = "AIR_TABLE_MANUAL_SOURCE";
        }

        public const string LISTING_CEX = "LISTING_CEX";
        public const string SPONSORED_TWEETS = "SPONSORED_TWEETS";
        public const string JUST_AUDITED = "JUST_AUDITED";
        public const string JUST_LISTED_IN_COINGECKO = "JUST_LISTED_IN_COINGECKO";
        public const string JUST_LISTED_IN_COINMARKETCAP = "JUST_LISTED_IN_COINMARKETCAP";
        public const string UPCOMMING_TOKEN_SALE = "UPCOMMING_TOKEN_SALE";
        public const string HOSTING_GIVEAWAYS = "HOSTING_GIVEAWAYS";
        public const string BUYING_SPONSORED_POSTS = "BUYING_SPONSORED_POSTS";

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
                case BUYING_SPONSORED_POSTS:
                    name = "Buying Sponsored Posts";
                    break;
                case JUST_LISTED_IN_COINGECKO:
                    name = "Just listed in Coingecko";
                    break;
                case JUST_LISTED_IN_COINMARKETCAP:
                    name = "Just listed in Coinmarketcap";
                    break;
                default:
                    name = null;
                    break;
            }

            return name;
        }

        public static string GetCode(string signal)
        {
            string code;
            switch (signal.Trim().ToLower())
            {
                case "listing cex":
                    code = LISTING_CEX;
                    break;
                case "audit is completed":
                    code = JUST_AUDITED;
                    break;
                case "buying sponsored ads":
                    code = SPONSORED_TWEETS;
                    break;
                case "upcomming token sales":
                    code = UPCOMMING_TOKEN_SALE;
                    break;
                case "hosting giveaways":
                    code = HOSTING_GIVEAWAYS;
                    break;
                case "buying sponsored posts":
                    code = BUYING_SPONSORED_POSTS;
                    break;
                case "Just listed in Coingecko":
                    code = JUST_LISTED_IN_COINGECKO;
                    break;
                case "Just listed in Coinmarketcap":
                    code = JUST_LISTED_IN_COINMARKETCAP;
                    break;
                default:
                    code = null;
                    break;
            }

            return code;
        }

    }

    public static class TwitterUser
    {
        public const string BOT_OWNER_NEW_LISTING_CMC_CGK_USER_ID = "1456211881035644935";
        public const string BOT_NEW_LISTING_CMC_CGK_USER_ID = "1688991175867502593";
    }
}
