using Medallion.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet;
using TK.Twitter.Crawl.Tweet.AirTable;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Jobs
{
    public class AirTableProcessWaitingJobArg
    {

    }

    public class AirTableProcessWaitingJob : AsyncBackgroundJob<AirTableProcessWaitingJobArg>, ITransientDependency
    {
        private const string LOG_PREFIX = "[AirTableProcessWaitingJob] ";
        public const int BATCH_SIZE = 10;

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<AirTableWaitingProcessEntity, long> _airTableWaitingProcessRepository;
        private readonly IRepository<AirTableLeadRecordMappingEntity, long> _airTableLeadRecordMappingepository;
        private readonly IRepository<LeadEntity, long> _leadRepository;
        private readonly AirTableManager _airTableManager;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedLockProvider _distributedLockProvider;

        public AirTableProcessWaitingJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<AirTableWaitingProcessEntity, long> airTableWaitingProcessRepository,
            IRepository<AirTableLeadRecordMappingEntity, long> airTableLeadRecordMappingepository,
            IRepository<LeadEntity, long> leadRepository,
            AirTableManager airTableManager,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedLockProvider distributedLockProvider)
        {
            _backgroundJobManager = backgroundJobManager;
            _airTableWaitingProcessRepository = airTableWaitingProcessRepository;
            _airTableLeadRecordMappingepository = airTableLeadRecordMappingepository;
            _leadRepository = leadRepository;
            _airTableManager = airTableManager;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedLockProvider = distributedLockProvider;
        }

        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(AirTableProcessWaitingJobArg args)
        {
            await using (var handle = await _distributedLockProvider.TryAcquireLockAsync($"AirTableProcessWaitingJob"))
            {
                if (handle == null)
                {
                    Logger.LogInformation(LOG_PREFIX + "Some process is running!!!");
                    return;
                }

                using var uow = _unitOfWorkManager.Begin();
                try
                {
                    var asyncExecuter = _airTableWaitingProcessRepository.AsyncExecuter;

                    var queueQuery = from input in await _airTableWaitingProcessRepository.GetQueryableAsync()
                                     where input.Ended == false
                                     select input;

                    // xử lý theo chiều oldest -> lastest
                    queueQuery = queueQuery.OrderBy(x => x.CreationTime).Take(BATCH_SIZE);

                    var queues = await asyncExecuter.ToListAsync(queueQuery);
                    if (queues.IsEmpty())
                    {
                        return;
                    }

                    var syncLeads = await asyncExecuter.ToListAsync(
                            from l in await _leadRepository.GetQueryableAsync()
                            join m in await _airTableLeadRecordMappingepository.GetQueryableAsync() on l.UserId equals m.ProjectUserId
                            into recordMap
                            from r in recordMap.DefaultIfEmpty()
                            where queues.Select(q => q.UserId).Contains(l.UserId)
                            select new
                            {
                                Lead = l,
                                AirTableRecordId = r.AirTableRecordId
                            }
                        );

                    foreach (var item in queues)
                    {
                        bool succeed = false;
                        string error;
                        try
                        {
                            var lead = syncLeads.FirstOrDefault(x => x.Lead.UserId == item.UserId);
                            if (item.Action == "CREATE")
                            {
                                (succeed, error) = await _airTableManager.AddLeadAsync(lead.Lead);
                            }
                            else
                            {
                                (succeed, error) = await _airTableManager.UpdateLeadAsync(lead.AirTableRecordId, lead.Lead);
                            }

                            item.Succeed = succeed;
                            if (!succeed)
                            {
                                item.Note = error;
                            }
                        }
                        catch (Exception ex)
                        {
                            item.Note = ex.ToString();
                        }

                        item.Ended = true;
                        await _airTableWaitingProcessRepository.UpdateAsync(item);
                    }

                    await uow.SaveChangesAsync();
                    await uow.CompleteAsync();
                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync();
                    Logger.LogError(ex, LOG_PREFIX + "An error occurred while crawling twitter data");
                }
            }

            await _backgroundJobManager.EnqueueAsync(args);
        }

    }
}
