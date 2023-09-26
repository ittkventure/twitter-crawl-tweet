using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.AlphaQuest;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Repository;
using TK.TwitterAccount.Domain;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Jobs
{
    public class TwitterTweetPrepareDataCrawlJobArg
    {

    }

    public class TwitterTweetPrepareDataCrawlJob : AsyncBackgroundJob<TwitterTweetPrepareDataCrawlJobArg>, ITransientDependency
    {
        private const string LOG_PREFIX = "[TwitterTweetPrepareDataCrawlJob] ";

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<TwitterTweetCrawlBatchEntity, long> _twitterFollowingCrawlBatchRepository;
        private readonly IClock _clock;
        private readonly ITwitterAccountRepository _twitterAccountRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<TwitterInfluencerEntity, long> _twitterInfluencerRepository;
        private readonly ITwitterCrawlRelationDapperRepository _twitterCrawlRelationDapperRepository;

        public TwitterTweetPrepareDataCrawlJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<TwitterTweetCrawlBatchEntity, long> twitterFollowingCrawlBatchRepository,
            IClock clock,
            ITwitterAccountRepository twitterAccountRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<TwitterInfluencerEntity, long> twitterInfluencerRepository,
            ITwitterCrawlRelationDapperRepository twitterCrawlRelationDapperRepository)
        {
            _backgroundJobManager = backgroundJobManager;
            _twitterFollowingCrawlBatchRepository = twitterFollowingCrawlBatchRepository;
            _clock = clock;
            _twitterAccountRepository = twitterAccountRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _twitterInfluencerRepository = twitterInfluencerRepository;
            _twitterCrawlRelationDapperRepository = twitterCrawlRelationDapperRepository;
        }

        public override async Task ExecuteAsync(TwitterTweetPrepareDataCrawlJobArg args)
        {
            using var uow = _unitOfWorkManager.Begin();

            // Lấy các Twitter Account đang hoạt động
            var crawlAccounts = await _twitterAccountRepository.GetListAsync(x => x.Enabled == true);
            if (crawlAccounts.IsEmpty())
            {
                return;
            }

            var executionTime = _clock.Now;
            var batchKey = executionTime.ToString(TwitterTweetCrawlBatchEntity.KEY_FORMAT);

            // Thực hiện đưa các influencer vào queue và chia các Crawl account xử lý
            try
            {
                var influencers = await _twitterInfluencerRepository.AsyncExecuter.ToListAsync(

                    from i in await _twitterInfluencerRepository.GetQueryableAsync()

                        //where i.Tags.Contains("audit") || i.Tags.Contains("cex")
                    //where i.CreationTime > executionTime.AddDays(-1)

                    select new
                    {
                        i.UserId,
                        i.Tags,
                    }
                    );

                if (influencers.IsEmpty())
                {
                    Logger.LogInformation(LOG_PREFIX + "Influencer data is empty");
                    return;
                }

                var batch = new TwitterTweetCrawlBatchEntity()
                {
                    CrawlTime = executionTime,
                    Key = batchKey,
                };

                int currentAccountIdx = 0;
                foreach (var influencer in influencers)
                {
                    var queue = new TwitterTweetCrawlQueueEntity()
                    {
                        UserId = influencer.UserId,
                        Ended = false,
                        BatchKey = batchKey,
                        Tags = influencer.Tags
                    };

                    var accountId = crawlAccounts[currentAccountIdx].AccountId;
                    currentAccountIdx++;

                    // Quay vòng lại account ID
                    if (currentAccountIdx >= crawlAccounts.Count)
                    {
                        currentAccountIdx = 0;
                    }

                    queue.TwitterAccountId = accountId;

                    batch.AddQueueItem(queue);
                }

                await _twitterFollowingCrawlBatchRepository.InsertAsync(batch);
                await uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                await uow.RollbackAsync();
                Logger.LogError(ex, LOG_PREFIX + "An error occurred when create crawl history");

                // k insert được dữ liệu thì bỏ qua luôn vì sang job crawl cũng k có gì để crawl cả
                return;
            }

            // Bắt đầu khởi động job crawl riêng của từng Crawl Account
            foreach (var item in crawlAccounts)
            {
                await _backgroundJobManager.EnqueueAsync(new TwitterTweetCrawlJobArg()
                {
                    BatchKey = batchKey,
                    TwitterAccountId = item.AccountId,
                });
            }
        }
    }
}
