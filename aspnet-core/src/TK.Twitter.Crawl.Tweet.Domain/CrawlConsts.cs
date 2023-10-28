using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Volo.Abp;
using static TK.Twitter.Crawl.CrawlConsts.Payment;

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

    public static class Payment
    {
        public const string FREE = "FREE";
        public const string LEAD3_STD_MONTHLY = "StandardMonthly";
        public const string LEAD3_STD_ANNUALLY = "StandardAnnually";
        public const string LEAD3_PRE_MONTHLY = "PremiumMonthly";
        public const string LEAD3_PRE_ANNUALLY = "PremiumAnnually";
        public const string LEAD3_TRIAL = "Trial";

        public static List<string> PAID_PLAN = new List<string>() { LEAD3_STD_MONTHLY, LEAD3_STD_ANNUALLY, LEAD3_PRE_MONTHLY, LEAD3_PRE_ANNUALLY };

        public static bool IsPremiumPlan(string key)
        {
            return key == LEAD3_PRE_ANNUALLY || key == LEAD3_PRE_MONTHLY;
        }

        public static bool IsStandardPlan(string key)
        {
            return key == LEAD3_STD_ANNUALLY || key == LEAD3_STD_MONTHLY;
        }

        public static bool IsTrialPlan(string key)
        {
            return key == LEAD3_TRIAL;
        }

        public static bool CheckValid(string planKey)
        {
            switch (planKey)
            {
                case FREE:
                case LEAD3_TRIAL:
                case LEAD3_STD_MONTHLY:
                case LEAD3_STD_ANNUALLY:
                case LEAD3_PRE_MONTHLY:
                case LEAD3_PRE_ANNUALLY:
                    return true;
                default:
                    return false;
            }
        }

        public static class Type
        {
            public const string CANCEL = "CANCEL";
            public const string UPGRADE_OR_RENEWAL = "UPGRADE_OR_RENEWAL";
        }
    }

    public static class Paddle
    {
        public class PaddlePlanConfig
        {
            public string Key { get; set; }
            public long Id { get; set; }
            public int RecurringIntervalMonth { get; set; }
            public decimal Price { get; set; }
        }

        public static List<PaddlePlanConfig> PaddlePlans = new List<PaddlePlanConfig>();

        public static void LoadPaddlePlan(IConfiguration configuration)
        {
            if (PaddlePlans.IsNotEmpty())
            {
                return;
            }

            var planconfigs = configuration.GetSection("RemoteServices:Paddle:Plans");
            foreach (var cfg in planconfigs.GetChildren())
            {
                var planConfig = new PaddlePlanConfig()
                {
                    Key = cfg.Key,
                    Id = cfg.GetValue<long>("PlanId"),
                    Price = cfg.GetValue<decimal>("Price"),
                    RecurringIntervalMonth = cfg.GetValue<int>("RecurringInterval"),
                };

                PaddlePlans.Add(planConfig);
            }
        }

        public static long GetPaddlePlanId(string planKey, IConfiguration configuration)
        {
            LoadPaddlePlan(configuration);
            var plan = PaddlePlans.FirstOrDefault(x => x.Key == planKey);
            return plan.Id;
        }

        public static string GetPaddlePlanKey(long planId, IConfiguration configuration)
        {
            LoadPaddlePlan(configuration);
            var plan = PaddlePlans.FirstOrDefault(x => x.Id == planId);
            return plan.Key;
        }

        public static bool IsPaddleStandardPlan(long planId, IConfiguration configuration)
        {
            LoadPaddlePlan(configuration);
            var plan = PaddlePlans.FirstOrDefault(x => x.Id == planId);
            return plan.Key == LEAD3_STD_MONTHLY || plan.Key == LEAD3_STD_ANNUALLY;
        }

        public static decimal GetPaddlePlanPrice(string planKey, IConfiguration configuration)
        {
            LoadPaddlePlan(configuration);
            var plan = PaddlePlans.FirstOrDefault(x => x.Key == planKey);
            return plan.Price;
        }

        public static DateTime GetPaddlePlanExpiredAt(string planKey, DateTime now, int paddingHours, IConfiguration configuration)
        {
            LoadPaddlePlan(configuration);
            var plan = PaddlePlans.FirstOrDefault(x => x.Key == planKey);
            return now.AddMonths(plan.RecurringIntervalMonth).AddHours(paddingHours);
        }

        public static (string, int) GetPlanRecurringInterval(string planKey, IConfiguration configuration)
        {
            if (IsTrialPlan(planKey))
            {
                return ("DAY", 3);
            }

            LoadPaddlePlan(configuration);
            var plan = PaddlePlans.FirstOrDefault(x => x.Key == planKey);
            return ("MONTH", plan.RecurringIntervalMonth);
        }
    }

    public static class CoinBase
    {
        public class CoinBasePlanConfig
        {
            public string Key { get; set; }
            public string Id { get; set; }
            public int RecurringIntervalMonth { get; set; }
            public decimal Price { get; set; }
        }

        public static List<CoinBasePlanConfig> CoinBasePlans = new List<CoinBasePlanConfig>();

        public static void LoadPlan(IConfiguration configuration)
        {
            if (CoinBasePlans.IsNotEmpty())
            {
                return;
            }
            
            var planconfigs = configuration.GetSection("RemoteServices:CoinBase:Plans");
            foreach (var cfg in planconfigs.GetChildren())
            {
                var planConfig = new CoinBasePlanConfig()
                {
                    Key = cfg.Key,
                    Id = cfg.GetValue<string>("PlanId"),
                    Price = cfg.GetValue<decimal>("Price"),
                    RecurringIntervalMonth = cfg.GetValue<int>("RecurringInterval"),
                };

                CoinBasePlans.Add(planConfig);
            }
        }

        public static string GetPlanKey(string planId, IConfiguration configuration)
        {
            LoadPlan(configuration);
            var plan = CoinBasePlans.FirstOrDefault(x => x.Id == planId);
            return plan.Key;
        }

        public static bool IsStandardPlan(string planId, IConfiguration configuration)
        {
            LoadPlan(configuration);
            var plan = CoinBasePlans.FirstOrDefault(x => x.Id == planId);
            return plan.Key == LEAD3_STD_MONTHLY || plan.Key == LEAD3_STD_ANNUALLY;
        }

        public static decimal GetPlanPrice(string planKey, IConfiguration configuration)
        {
            LoadPlan(configuration);
            var plan = CoinBasePlans.FirstOrDefault(x => x.Key == planKey);
            return plan.Price;
        }

        public static (string, int) GetPlanRecurringInterval(string planKey, IConfiguration configuration)
        {
            if (IsTrialPlan(planKey))
            {
                return ("DAY", 3);
            }

            LoadPlan(configuration);
            var plan = CoinBasePlans.FirstOrDefault(x => x.Key == planKey);
            return ("MONTH", plan.RecurringIntervalMonth);
        }

    }
}
