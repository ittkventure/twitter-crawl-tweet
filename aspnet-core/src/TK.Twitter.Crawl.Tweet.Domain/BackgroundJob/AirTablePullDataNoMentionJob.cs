using AirtableApiClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Notification;
using TK.Twitter.Crawl.Tweet.AirTable;
using TK.Twitter.Crawl.Tweet.MemoryLock;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Jobs
{
    public class AirTableNoMentionPullDataJobArg
    {
        public string Offset { get; set; }
    }

    public class AirTableNoMentionPullDataJob : AsyncBackgroundJob<AirTableNoMentionPullDataJobArg>, ITransientDependency
    {
        private const string LOG_PREFIX = "[AirTableNoMentionPullDataJob] ";
        public const int BATCH_SIZE = 10;

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<AirTableNoMentionWaitingProcessEntity, long> _airTableNoMentionWaitingProcessRepository;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly AirTableService _airTableService;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly MemoryLockProvider _memoryLockProvider;

        public AirTableNoMentionPullDataJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<AirTableNoMentionWaitingProcessEntity, long> airTableNoMentionWaitingProcessRepository,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            AirTableService airTableService,
            IDistributedEventBus distributedEventBus,
            ILogger<AirTableNoMentionPullDataJob> logger,
            MemoryLockProvider memoryLockProvider)
        {
            _backgroundJobManager = backgroundJobManager;
            _airTableNoMentionWaitingProcessRepository = airTableNoMentionWaitingProcessRepository;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _airTableService = airTableService;
            _distributedEventBus = distributedEventBus;
            _memoryLockProvider = memoryLockProvider;
            Logger = logger;
        }

        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(AirTableNoMentionPullDataJobArg args)
        {
            using (var handle = _memoryLockProvider.TryAcquireLock($"AirTableNoMentionPullDataJob"))
            {
                if (handle == null)
                {
                    Logger.LogInformation(LOG_PREFIX + "Some process is running!!!");
                    return;
                }

                using var uow = _unitOfWorkManager.Begin();
                try
                {
                    var pr = await _airTableService.GetPaged<Dictionary<string, object>>(
                        AirTableNoMentionManager.TABLE_NAME,
                        args.Offset,
                        pageSize: 100,
                        fields: AirTableNoMentionManager.PULL_FIELDS,
                        //fields: null,
                        sort: new List<Sort>() { new ()
                                {
                                    Field = "Lastest Signal Date",
                                    Direction = SortDirection.Desc
                                }
                        });

                    args.Offset = pr.Offset;

                    if (!pr.Success)
                    {
                        string error;
                        if (pr.AirtableApiError is AirtableApiException)
                        {
                            error = pr.AirtableApiError.ErrorMessage;
                            if (pr.AirtableApiError is AirtableInvalidRequestException)
                            {
                                error += pr.AirtableApiError.DetailedErrorMessage;
                            }
                        }
                        else
                        {
                            error = "Unknown error";
                        }

                        await _distributedEventBus.PublishAsync(new NotificationErrorEto()
                        {
                            Tags = "[Warning][AirTableNoMentionPullDataJob]",
                            Message = $"Có lỗi trong quá trình đồng bộ dữ liệu từ AirTable",
                            ExceptionStackTrace = error
                        });

                        throw new BusinessException(CrawlDomainErrorCodes.InsideLogicError, error);
                    }

                    if (pr.Records.IsEmpty())
                    {
                        Logger.LogInformation(LOG_PREFIX + "success");
                        return;
                    }

                    foreach (var item in pr.Records)
                    {
                        if (item.Fields.Count != AirTableNoMentionManager.PULL_FIELDS.Count)
                        {
                            continue;
                        }

                        await _airTableNoMentionWaitingProcessRepository.InsertAsync(new()
                        {
                            Action = "PULL",
                            RefId = item.Id,
                            Ref2 = item.Fields["Project Twitter"]?.ToString(),
                        });
                    }

                    await uow.SaveChangesAsync();
                    await uow.CompleteAsync();

                    if (pr.Offset.IsNotEmpty())
                    {
                        await _backgroundJobManager.EnqueueAsync(args);
                    }
                    else
                    {
                        Logger.LogInformation(LOG_PREFIX + "success");
                    }
                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync();
                    Logger.LogError(ex, LOG_PREFIX + "Sync data from AirTable Manual Source");
                }
            }
        }
    }
}
