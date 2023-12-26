using AirtableApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Jobs;
using TK.Twitter.Crawl.Notification;
using TK.Twitter.Crawl.Tweet;
using TK.Twitter.Crawl.Tweet.AirTable;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.BackgroundWorkers
{
    public class AirTableNoMentionProcessWaitingWorker : ITransientDependency
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public AirTableNoMentionProcessWaitingWorker(
            IBackgroundJobManager backgroundJobManager,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _backgroundJobManager = backgroundJobManager;
            Clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public IClock Clock { get; }

        [UnitOfWork(IsDisabled = true)]
        public async Task DoWorkAsync()
        {
            using var uow = _unitOfWorkManager.Begin();
            try
            {
                await _backgroundJobManager.EnqueueAsync(new AirTableNoMentionProcessWaitingJobArg());
                await uow.CompleteAsync();
            }
            catch
            {
                await uow.RollbackAsync();
            }
        }
    }
}
