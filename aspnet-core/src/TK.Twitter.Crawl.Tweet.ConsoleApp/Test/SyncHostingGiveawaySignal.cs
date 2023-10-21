using System.Threading.Tasks;
using System.Linq;
using TK.Twitter.Crawl.Entity;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using System.Collections.Generic;
using Volo.Abp.BackgroundJobs;
using System;
using TK.Twitter.Crawl.Jobs;
using TK.Twitter.Crawl.Tweet;
using System.Collections;
using TK.Twitter.Crawl.Tweet.AirTable;
using Volo.Abp.ObjectExtending.Modularity;
using static Volo.Abp.Identity.IdentityPermissions;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{

    public class SyncHostingGiveawaySignal : ITransientDependency
    {
        private readonly IRepository<TwitterTweetHashTagEntity, long> _twitterTweetHashTagRepository;
        private readonly IRepository<TwitterInfluencerEntity, long> _twitterInfluencerRepository;
        private readonly IRepository<TwitterTweetEntity, long> _twitterTweetRepository;
        private readonly IRepository<TwitterTweetUserMentionEntity, long> _twitterTweetUserMentionRepository;
        private readonly IRepository<TwitterUserSignalEntity, long> _twitterUserSignalRepository;
        private readonly IRepository<LeadWaitingProcessEntity, long> _leadWaitingProcessRepository;
        private readonly IRepository<TwitterUserTypeEntity, long> _twitterUserTypeRepository;
        private readonly IRepository<TwitterUserStatusEntity, long> _twitterUserStatusRepository;
        private readonly IRepository<LeadEntity, long> _leadRepository;
        private readonly IRepository<AirTableLeadRecordMappingEntity, long> _airTableLeadRecordMappingRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly AirTableLead3Manager _airTableLead3Manager;
        private readonly Lead3Manager _lead3Manager;

        public SyncHostingGiveawaySignal(
            IRepository<TwitterTweetHashTagEntity, long> twitterTweetHashTagRepository,
            IRepository<TwitterInfluencerEntity, long> twitterInfluencerRepository,
            IRepository<TwitterTweetEntity, long> twitterTweetRepository,
            IRepository<TwitterTweetUserMentionEntity, long> twitterTweetUserMentionRepository,
            IRepository<TwitterUserSignalEntity, long> twitterUserSignalRepository,
            IRepository<LeadWaitingProcessEntity, long> leadWaitingProcessRepository,
            IRepository<TwitterUserTypeEntity, long> twitterUserTypeRepository,
            IRepository<TwitterUserStatusEntity, long> twitterUserStatusRepository,
            IRepository<LeadEntity, long> leadRepository,
            IRepository<AirTableLeadRecordMappingEntity, long> airTableLeadRecordMappingRepository,
            IBackgroundJobManager backgroundJobManager,
            AirTableLead3Manager airTableLead3Manager,
            Lead3Manager lead3Manager)
        {
            _twitterTweetHashTagRepository = twitterTweetHashTagRepository;
            _twitterInfluencerRepository = twitterInfluencerRepository;
            _twitterTweetRepository = twitterTweetRepository;
            _twitterTweetUserMentionRepository = twitterTweetUserMentionRepository;
            _twitterUserSignalRepository = twitterUserSignalRepository;
            _leadWaitingProcessRepository = leadWaitingProcessRepository;
            _twitterUserTypeRepository = twitterUserTypeRepository;
            _twitterUserStatusRepository = twitterUserStatusRepository;
            _leadRepository = leadRepository;
            _airTableLeadRecordMappingRepository = airTableLeadRecordMappingRepository;
            _backgroundJobManager = backgroundJobManager;
            _airTableLead3Manager = airTableLead3Manager;
            _lead3Manager = lead3Manager;
        }

        public async Task RunAsync()
        {
            var query = from q in await _twitterUserSignalRepository.GetQueryableAsync()
                        where q.Signal == CrawlConsts.Signal.JUST_LISTED_IN_COINMARKETCAP || q.Signal == CrawlConsts.Signal.JUST_LISTED_IN_COINGECKO
                        select q.UserId;

            query = query.Distinct();

            var userIds = await _airTableLeadRecordMappingRepository.AsyncExecuter.ToListAsync(query);
            
            // để job tự update
            foreach (var userId in userIds)
            {
                await _leadWaitingProcessRepository.InsertAsync(new LeadWaitingProcessEntity()
                {
                    BatchKey = "UPDATE_OTHER_SIGNAL_COL",
                    UserId = userId,
                    Source = CrawlConsts.Signal.Source.TWITTER_TWEET
                });
            }
        }

        public async Task DeleteSignalWrong_RunAsync()
        {
            var userIds = new List<string>() { "81834205", "363770078", "634075747", "724550786", "2364065785", "3296780478", "897637919510409218", "937166242208763904", "951252339599577088", "953559767242498049", "970698661364715520", "984640069171544065", "1052454006537314306", "1060756392733102081", "1078130452202835970", "1153288070982225920", "1245086198386786305", "1265346267300786176", "1296337092302139396", "1300649161025400832", "1300960681148264455", "1344633325609054208", "1357395816193916929", "1358640864482889729", "1358688239037546500", "1372924126566912005", "1376761856749006848", "1390323555569524738", "1390586301565394944", "1393207449965023232", "1395846093389316096", "1398219687079538689", "1399666762723905536", "1403345596702855168", "1409359424989196292", "1416470987701465091", "1426521558546194435", "1433389057799860225", "1437995145371086848", "1449810845987397636", "1451089087377522689", "1459046577306173440", "1467518903404437507", "1470785197083967510", "1475686491040985094", "1477777674307264513", "1484186921849208832", "1484749104672800769", "1502073783615688711", "1508194547632951296", "1514276588804001793", "1516445558193459202", "1530456585348296705", "1544044068413521922", "1573182156406288384", "1621192054956244994" };
            var leads = await _lead3Manager.GetLeadsAsync(userIds);
            var dbLeads = await _leadRepository.GetListAsync(x => userIds.Contains(x.UserId));

            //var check = leads.GroupBy(x => x.UserId).Where(x => x.Count() > 1).Select(x => new { x.Key, Count = x.Count() }).ToList();

            //// để job tự update
            //foreach (var lead in leads)
            //{
            //    await _leadWaitingProcessRepository.InsertAsync(new LeadWaitingProcessEntity()
            //    {
            //        BatchKey = "UPDATE_WRONG_SIGNAL",
            //        UserId = lead.UserId,
            //    });
            //}

            // xóa lead
            var deleteLeads = dbLeads.Where(x => !leads.Any(l => l.UserId == x.UserId)).ToList();
            var mappings = await _airTableLeadRecordMappingRepository.GetListAsync(x => deleteLeads.Select(x => x.UserId).Contains(x.ProjectUserId));

            if (deleteLeads.IsNotEmpty())
            {
                for (int i = 0; i < deleteLeads.Count; i++)
                {
                    Console.WriteLine($"{i + 1}/{deleteLeads.Count}");
                    var delete = deleteLeads[i];
                    var mapping = mappings.FirstOrDefault(x => x.ProjectUserId == delete.UserId);
                    var error = await _airTableLead3Manager.DeleteAsync(mapping.AirTableRecordId);
                    if (error.IsNotEmpty())
                    {

                    }

                    await _leadRepository.HardDeleteAsync(delete);
                    await _airTableLeadRecordMappingRepository.HardDeleteAsync(mappings);
                }
            }
        }

        public async Task AddSignal_RunAsync()
        {
            var influencerQuery = await _twitterInfluencerRepository.GetQueryableAsync();
            var mentionQuery = from um in await _twitterTweetUserMentionRepository.GetQueryableAsync()
                               join t in await _twitterTweetRepository.GetQueryableAsync() on um.TweetId equals t.TweetId
                               join ht in await _twitterTweetHashTagRepository.GetQueryableAsync() on t.TweetId equals ht.TweetId
                               where
                               influencerQuery.Any(x => x.UserId == t.UserId && (x.Tags == null || x.Tags == ""))
                               && (ht.NormalizeText.Contains("giveaways") || ht.NormalizeText.Contains("giveaway") || ht.NormalizeText.Contains("airdrops") || ht.NormalizeText.Contains("airdrop") || ht.NormalizeText.Contains("gleam"))
                               select um;

            var userMention = await _twitterInfluencerRepository.AsyncExecuter.ToListAsync(mentionQuery);

            for (int i = 0; i < userMention.Count; i++)
            {
                Console.WriteLine($"{i + 1}/ {userMention.Count}");
                var um = userMention[i];

                await _twitterUserSignalRepository.InsertAsync(new TwitterUserSignalEntity()
                {
                    UserId = um.UserId,
                    TweetId = um.TweetId,
                    Signal = "HOSTING_GIVEAWAYS",
                    Source = CrawlConsts.Signal.Source.TWITTER_TWEET
                });

                await _leadWaitingProcessRepository.InsertAsync(new LeadWaitingProcessEntity()
                {
                    BatchKey = "SYNC_HOSTING_GIVEAWAYS",
                    UserId = um.UserId,
                    TweetId = um.TweetId,
                    Source = CrawlConsts.Signal.Source.TWITTER_TWEET
                });

                var userType = await _twitterUserTypeRepository.FirstOrDefaultAsync(x => x.UserId == um.UserId);
                if (userType == null)
                {
                    await _twitterUserTypeRepository.InsertAsync(new TwitterUserTypeEntity()
                    {
                        UserId = um.UserId,
                        Type = CrawlConsts.LeadType.LEADS,
                        IsUserSuppliedValue = false,
                    }, autoSave: true);
                }
                else
                {
                    if (!userType.IsUserSuppliedValue && userType.Type != CrawlConsts.LeadType.LEADS)
                    {
                        userType.Type = CrawlConsts.LeadType.LEADS;
                        await _twitterUserTypeRepository.UpdateAsync(userType);
                    }
                }

                var userStatus = await _twitterUserStatusRepository.FirstOrDefaultAsync(x => x.UserId == um.UserId);
                if (userStatus == null)
                {
                    await _twitterUserStatusRepository.InsertAsync(new TwitterUserStatusEntity()
                    {
                        UserId = um.UserId,
                        Status = "New",
                        IsUserSuppliedValue = false,
                    }, autoSave: true);
                }
                else
                {
                    if (!userStatus.IsUserSuppliedValue && userStatus.Status != "New")
                    {
                        userStatus.Status = "New";
                        await _twitterUserStatusRepository.UpdateAsync(userStatus);
                    }
                }
            }
        }
    }
}
