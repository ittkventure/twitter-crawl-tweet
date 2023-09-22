using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Jobs;
using TK.Twitter.Crawl.Notification;
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
                var batchQuery = await _twitterTweetCrawlBatchRepository.GetQueryableAsync();
                batchQuery = batchQuery.OrderByDescending(x => x.CreationTime);

                var lastBatch = await _twitterTweetCrawlBatchRepository.AsyncExecuter.FirstOrDefaultAsync(batchQuery);

                // Nếu chưa thực hiện batch nào thì cho start job craw luôn
                if (lastBatch == null)
                {
                    await StartCrawl();
                }
                else
                {
                    var notEndQueue = await _twitterTweetCrawlQueueRepository.AnyAsync(x => x.BatchKey == lastBatch.Key && x.Ended == false);
                    // đã crawl đủ dữ liệu nên chuyển sang crawl batch khác
                    if (!notEndQueue)
                    {
                        await StartCrawl();
                    }
                    else
                    {
                        // Nếu các queue chưa được thực hiện hết mà thời gian thực hiện job lớn hơn 1 ngày
                        if (lastBatch.CrawlTime < Clock.Now.AddDays(-1))
                        {
                            var count = await _twitterTweetCrawlQueueRepository.CountAsync(x => x.BatchKey == lastBatch.Key && x.Ended == false);
                            await _distributedEventBus.PublishAsync(new NotificationErrorEto()
                            {
                                Tags = "[Warning][TwitterTweetCrawlWorker]",
                                Message = $"Batch {lastBatch.Key} đã chạy lâu hơn 1 ngày. Số lượng tồn: {count}. Kiểm tra đi man!!!"
                            });
                        }
                    }
                }

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
