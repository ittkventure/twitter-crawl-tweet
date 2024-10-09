using AirtableApiClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AirTableCheckDataJobArg
    {
        public string Offset { get; set; }
    }

    public class AirTableCheckDataJob : AsyncBackgroundJob<AirTableCheckDataJobArg>, ITransientDependency
    {
        private const string LOG_PREFIX = "[AirTableCheckDataJob] ";
        public const int BATCH_SIZE = 10;

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<AirTableLeadRecordMappingEntity, long> _airTableLeadRecordMappingepository;
        private readonly IRepository<TwitterUserTypeEntity, long> _twitterUserTypepository;
        private readonly IRepository<LeadEntity, long> _leadRepository;
        private readonly AirTableLead3Manager _airTableManager;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly AirTableLead3Service _airTableService;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly MemoryLockProvider _memoryLockProvider;

        public AirTableCheckDataJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<AirTableLeadRecordMappingEntity, long> airTableLeadRecordMappingepository,
            IRepository<TwitterUserTypeEntity, long> twitterUserTypepository,
            IRepository<LeadEntity, long> leadRepository,
            AirTableLead3Manager airTableManager,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            AirTableLead3Service airTableService,
            IDistributedEventBus distributedEventBus,
            ILogger<AirTableCheckDataJob> logger,
            MemoryLockProvider memoryLockProvider)
        {
            _backgroundJobManager = backgroundJobManager;
            _airTableLeadRecordMappingepository = airTableLeadRecordMappingepository;
            _twitterUserTypepository = twitterUserTypepository;
            _leadRepository = leadRepository;
            _airTableManager = airTableManager;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _airTableService = airTableService;
            _distributedEventBus = distributedEventBus;
            _memoryLockProvider = memoryLockProvider;
            Logger = logger;
        }

        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(AirTableCheckDataJobArg args)
        {
            using (var handle = _memoryLockProvider.TryAcquireLock($"AirTableCheckDataJob"))
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
                        AirTableLead3Manager.LEAD_TABLE_NAME,
                        args.Offset,
                        pageSize: 100,
                        fields: new List<string> { "Type" }
                        );

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
                            Tags = "[Warning][AirTableCheckDataJob]",
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

                    var leadQuery = from l in await _leadRepository.GetQueryableAsync()
                                    join m in await _airTableLeadRecordMappingepository.GetQueryableAsync() on l.UserId equals m.ProjectUserId
                                    where pr.Records.Select(x => x.Id).Contains(m.AirTableRecordId)
                                    select new
                                    {
                                        Lead = l,
                                        RecordId = m.AirTableRecordId
                                    };

                    var leads = await _leadRepository.AsyncExecuter.ToListAsync(leadQuery);
                    var userTypes = await _twitterUserTypepository.GetListAsync(x => leads.Select(u => u.Lead.UserId).Contains(x.UserId));

                    foreach (var item in pr.Records)
                    {
                        var lr = leads.FirstOrDefault(x => x.RecordId == item.Id);
                        if (lr == null)
                        {
                            Logger.LogError(LOG_PREFIX + $"[Record {item.Id}]" + "Lead not Found");
                            continue;
                        }

                        bool b = item.Fields.TryGetValue("Type", out object otype);
                        if (!b)
                        {
                            Logger.LogError(LOG_PREFIX + $"[Record {item.Id}]" + "Can not parse Type");
                            continue;
                        }

                        var type = otype.ToString();
                        if (!CrawlConsts.LeadType.AllowList.Contains(type))
                        {
                            Logger.LogError(LOG_PREFIX + $"[Record {item.Id}]" + "Type invalidated. Type: " + type);
                            continue;
                        }

                        if (lr.Lead.UserType != type)
                        {
                            lr.Lead.UserType = type;
                            await _leadRepository.UpdateAsync(lr.Lead);
                        }

                        var userType = userTypes.FirstOrDefault(x => x.UserId == lr.Lead.UserId);
                        if (userType == null)
                        {
                            await _twitterUserTypepository.InsertAsync(new TwitterUserTypeEntity()
                            {
                                UserId = lr.Lead.UserId,
                                Type = type,
                                IsUserSuppliedValue = true
                            }, autoSave: true);
                        }
                        else
                        {
                            if (userType.Type != type)
                            {
                                userType.Type = type;
                                userType.IsUserSuppliedValue = true;
                                await _twitterUserTypepository.UpdateAsync(userType);
                            }
                        }
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
                    Logger.LogError(ex, LOG_PREFIX + "Sync data from AirTable");
                }
            }
        }
    }
}
