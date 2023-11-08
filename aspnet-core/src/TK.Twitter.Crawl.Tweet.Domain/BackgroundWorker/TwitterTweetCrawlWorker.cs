using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<TwitterTweetCrawlQueueEntity, long> _twitterTweetCrawlQueueRepository;

        public TwitterTweetCrawlWorker(
            IBackgroundJobManager backgroundJobManager,
            IRepository<TwitterTweetCrawlQueueEntity, long> twitterTweetCrawlQueueRepository,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _backgroundJobManager = backgroundJobManager;
            Clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _twitterTweetCrawlQueueRepository = twitterTweetCrawlQueueRepository;
        }

        public IClock Clock { get; }

        [UnitOfWork(IsDisabled = true)]
        public async Task DoWorkAsync()
        {
            using var uow = _unitOfWorkManager.Begin();
            try
            {
                var waitingProcesses = await _twitterTweetCrawlQueueRepository.GetListAsync(x => x.Ended == false);
                if (waitingProcesses.IsEmpty())
                {
                    await StartCrawl();
                }
                else
                {
                    var groups = waitingProcesses.GroupBy(x => new { x.BatchKey, x.TwitterAccountId })
                                                 .Select(x => new { x.Key.BatchKey, x.Key.TwitterAccountId })
                                                 .OrderBy(x => x.BatchKey);
                    foreach (var grp in groups)
                    {
                        await _backgroundJobManager.EnqueueAsync(new TwitterTweetCrawlJobArg()
                        {
                            BatchKey = grp.BatchKey,
                            TwitterAccountId = grp.TwitterAccountId,
                        });
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
