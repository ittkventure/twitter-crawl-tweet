using AirtableApiClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet;
using TK.Twitter.Crawl.Tweet.AirTable;
using TK.Twitter.Crawl.TwitterAPI;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class TestAirTable : ITransientDependency
    {
        private readonly HttpClient _httpClient;
        private readonly AirTableLead3Manager _airTableManager;
        private readonly Lead3Manager _lead3Manager;
        private readonly IRepository<LeadEntity, long> _leadRepository;

        public TestAirTable(
            HttpClient httpClient,
            AirTableLead3Manager airTableManager,
            Lead3Manager lead3Manager,
            IRepository<LeadEntity, long> leadRepository)
        {
            _httpClient = httpClient;
            _airTableManager = airTableManager;
            _lead3Manager = lead3Manager;
            _leadRepository = leadRepository;
        }

        public async Task Test()
        {
            //var mentions = await _lead3Manager.GetLeadsAsync();
            //var leads = new List<LeadEntity>();
            //foreach (var lead in mentions)
            //{
            //    var entity = await _leadRepository.InsertAsync(new LeadEntity()
            //    {
            //        UserId = lead.UserId,
            //        UserName = lead.UserName,
            //        UserScreenName = lead.UserScreenName,
            //        UserProfileUrl = "https://twitter.com/i/user/" + lead.UserId,
            //        UserType = "LEADS",
            //        UserStatus = "NEW",
            //        Signals = lead.Signals?.JoinAsString(","),
            //        LastestTweetId = lead.LastestTweetId,
            //        LastestSponsoredDate = lead.LastestSponsoredDate,
            //        LastestSponsoredTweetUrl = lead.LastestSponsoredTweetUrl,
            //        DuplicateUrlCount = lead.DuplicateUrlCount,
            //        TweetDescription = lead.TweetDescription,
            //        NormalizeTweetDescription = lead.NormalizeTweetDescription,
            //        TweetOwnerUserId = lead.TweetOwnerUserId,
            //        MediaMentioned = lead.MediaMentioned,
            //        MediaMentionedProfileUrl = "https://twitter.com/i/user/" + lead.TweetOwnerUserId,
            //        NumberOfSponsoredTweets = lead.NumberOfSponsoredTweets,
            //        HashTags = lead.HashTags?.JoinAsString(","),
            //    });

            //    leads.Add(
            //       entity
            //    );
            //}

            var leads = await _leadRepository.GetListAsync();
            var error = await _airTableManager.BulkUpdateLeadAsync(leads);
        }
    }
}
