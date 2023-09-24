using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Jobs;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.BackgroundWorkers
{
    public class TwitterTweetCrawlWorker : ITransientDependency
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<TwitterTweetCrawlBatchEntity, long> _twitterTweetCrawlBatchRepository;
        private readonly IRepository<TwitterTweetCrawlQueueEntity, long> _twitterTweetCrawlQueueRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedEventBus _distributedEventBus;

        public TwitterTweetCrawlWorker(
            IBackgroundJobManager backgroundJobManager,
            IRepository<TwitterTweetCrawlBatchEntity, long> twitterTweetCrawlHistoryRepository,
            IRepository<TwitterTweetCrawlQueueEntity, long> twitterTweetCrawlQueueRepository,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedEventBus distributedEventBus
            )
        {
            _backgroundJobManager = backgroundJobManager;
            _twitterTweetCrawlBatchRepository = twitterTweetCrawlHistoryRepository;
            _twitterTweetCrawlQueueRepository = twitterTweetCrawlQueueRepository;
            Clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedEventBus = distributedEventBus;
        }

        public IClock Clock { get; }

        [UnitOfWork(IsDisabled = true)]
        public async Task DoWorkAsync()
        {
            using var uow = _unitOfWorkManager.Begin();

            try
            {
                await StartCrawl();
                await uow.SaveChangesAsync();
                await uow.CompleteAsync();
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }

        }

        private Task StartCrawl()
        {
            var arg = new TwitterTweetPrepareDataCrawlJobArg();
            return _backgroundJobManager.EnqueueAsync(arg);
        }
    }
}
