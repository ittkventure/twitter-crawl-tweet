using AirtableApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace TK.Twitter.Crawl.Tweet.AirTable
{
    public class AirTableManualSourceManager : DomainService
    {
        private readonly AirTableService _airTableService;
        private readonly IRepository<AirTableManualSourceEntity, long> _airTableManualSourceRepository;
        public const string TABLE_NAME = "Manual Sources";
        public static List<string> FIELDS = new List<string>()
        {
            "Project Twitter",
            "Type",
            "Signals",
            "Lastest Signal From",
            "Lastest Signal Date",
            "Lastest Signal Description",
            "Lastest Signal URL",
        };

        public AirTableManualSourceManager(
            AirTableService airTableService,
            IRepository<AirTableManualSourceEntity, long> airTableManualSourceRepository)
        {
            _airTableService = airTableService;
            _airTableManualSourceRepository = airTableManualSourceRepository;
        }

        public static Dictionary<string, object> GetAirTableLeadFields(LeadEntity lead, TwitterUserEntity user)
        {
            var dict = new Dictionary<string, object>()
            {
                { "Latest Signal HashTags", lead.HashTags },
                { "Type", lead.UserType },
                { "Lastest Tweet Id", lead.LastestTweetId },
                { "Lastest Signal Date", lead.LastestSponsoredDate },
                { "Latest Signal Description", lead.TweetDescription },
                { "Media Mentioned User Id", lead.TweetOwnerUserId },
                { "Latest signal from", lead.MediaMentioned },
                { "Duplicate Url Count", lead.DuplicateUrlCount },
                { "Latest Signal URL", lead.LastestSponsoredTweetUrl },
                { "Other Signals", lead.SignalDescription },
                { "Project Twitter", lead.UserProfileUrl },
                { "System Lead Id", lead.UserId },
            };

            if (user != null)
            {
                dict.Add("Project Twitter Bio", user.Description);
            }

            if (lead.Signals.IsNotEmpty())
            {
                var airTableSignals = new List<string>();
                foreach (var signal in lead.Signals.Split(","))
                {
                    string name = CrawlConsts.Signal.GetName(signal);
                    if (name.IsNotEmpty())
                    {
                        airTableSignals.Add(name);
                    }
                }

                dict.Add("Signals", airTableSignals);
            }

            return dict;
        }

    }
}
