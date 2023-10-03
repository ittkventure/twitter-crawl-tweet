using System.Threading.Tasks;
using System.Linq;
using TK.Twitter.Crawl.Entity;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using System.Collections.Generic;
using Volo.Abp.BackgroundJobs;
using System;

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
        private readonly IBackgroundJobManager _backgroundJobManager;

        public SyncHostingGiveawaySignal(
            IRepository<TwitterTweetHashTagEntity, long> twitterTweetHashTagRepository,
            IRepository<TwitterInfluencerEntity, long> twitterInfluencerRepository,
            IRepository<TwitterTweetEntity, long> twitterTweetRepository,
            IRepository<TwitterTweetUserMentionEntity, long> twitterTweetUserMentionRepository,
            IRepository<TwitterUserSignalEntity, long> twitterUserSignalRepository,
            IRepository<LeadWaitingProcessEntity, long> leadWaitingProcessRepository,
            IRepository<TwitterUserTypeEntity, long> twitterUserTypeRepository,
            IRepository<TwitterUserStatusEntity, long> twitterUserStatusRepository,
            IBackgroundJobManager backgroundJobManager)
        {
            _twitterTweetHashTagRepository = twitterTweetHashTagRepository;
            _twitterInfluencerRepository = twitterInfluencerRepository;
            _twitterTweetRepository = twitterTweetRepository;
            _twitterTweetUserMentionRepository = twitterTweetUserMentionRepository;
            _twitterUserSignalRepository = twitterUserSignalRepository;
            _leadWaitingProcessRepository = leadWaitingProcessRepository;
            _twitterUserTypeRepository = twitterUserTypeRepository;
            _twitterUserStatusRepository = twitterUserStatusRepository;
            _backgroundJobManager = backgroundJobManager;
        }

        public async Task RunAsync()
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
                    Signal = "HOSTING_GIVEAWAYS"
                });

                await _leadWaitingProcessRepository.InsertAsync(new LeadWaitingProcessEntity()
                {
                    BatchKey = "SYNC_HOSTING_GIVEAWAYS",
                    UserId = um.UserId,
                    TweetId = um.TweetId,
                });

                var userType = await _twitterUserTypeRepository.FirstOrDefaultAsync(x => x.UserId == um.UserId);
                if (userType == null)
                {
                    await _twitterUserTypeRepository.InsertAsync(new TwitterUserTypeEntity()
                    {
                        UserId = um.UserId,
                        Type = CrawlConsts.LeadType.LEADS,
                        IsUserSuppliedValue = false,
                    });
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
                    });
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
