using AirtableApiClient;
using Medallion.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Notification;
using TK.Twitter.Crawl.Tweet.AirTable;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using System.Collections;
using System.Text.Json;
using TK.Twitter.Crawl.Tweet.MemoryLock;

namespace TK.Twitter.Crawl.Jobs
{
    public class AirTableCheckDataManualSourceJobArg
    {
        public string Offset { get; set; }
    }

    public class AirTableCheckDataManualSourceJob : AsyncBackgroundJob<AirTableCheckDataManualSourceJobArg>, ITransientDependency
    {
        private const string LOG_PREFIX = "[AirTableCheckDataManualSourceJob] ";
        public const int BATCH_SIZE = 10;

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<AirTableManualSourceEntity, long> _airTableManualSourceRepository;
        private readonly IRepository<AirTableManualSourceWaitingProcessEntity, long> _airTableManualSourceWaitingProcessRepository;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly AirTableService _airTableService;
        private readonly IDistributedLockProvider _distributedLockProvider;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly MemoryLockProvider _memoryLockProvider;

        public AirTableCheckDataManualSourceJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<AirTableManualSourceEntity, long> airTableManualSourceRepository,
            IRepository<AirTableManualSourceWaitingProcessEntity, long> airTableManualSourceWaitingProcessRepository,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            AirTableService airTableService,
            IDistributedLockProvider distributedLockProvider,
            IDistributedEventBus distributedEventBus,
            ILogger<AirTableCheckDataManualSourceJob> logger,
            MemoryLockProvider memoryLockProvider)
        {
            _backgroundJobManager = backgroundJobManager;
            _airTableManualSourceRepository = airTableManualSourceRepository;
            _airTableManualSourceWaitingProcessRepository = airTableManualSourceWaitingProcessRepository;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _airTableService = airTableService;
            _distributedLockProvider = distributedLockProvider;
            _distributedEventBus = distributedEventBus;
            _memoryLockProvider = memoryLockProvider;
            Logger = logger;
        }

        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(AirTableCheckDataManualSourceJobArg args)
        {
            using (var handle = _memoryLockProvider.TryAcquireLock($"AirTableCheckDataManualSourceJob"))
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
                        AirTableManualSourceManager.TABLE_NAME,
                        args.Offset,
                        pageSize: 100,
                        fields: AirTableManualSourceManager.FIELDS
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
                            Tags = "[Warning][AirTableCheckDataManualSourceJob]",
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

                    var alreadyExists = await _airTableManualSourceRepository.GetListAsync(at => pr.Records.Select(x => x.Id).Contains(at.RecordId));
                    foreach (var item in pr.Records)
                    {
                        if (item.Fields.Count != AirTableManualSourceManager.FIELDS.Count)
                        {
                            continue;
                        }

                        bool change = false;
                        var db = alreadyExists.FirstOrDefault(x => x.RecordId == item.Id);
                        if (db == null)
                        {
                            db = new AirTableManualSourceEntity()
                            {
                                RecordId = item.Id,
                            };

                            (change, db) = ParseData(item.Fields, db);

                            await _airTableManualSourceRepository.InsertAsync(db);

                            // Thêm vào queue
                            await _airTableManualSourceWaitingProcessRepository.InsertAsync(new AirTableManualSourceWaitingProcessEntity()
                            {
                                RecordId = item.Id,
                                Action = "CREATE",
                                ProjectTwitter = db.ProjectTwitter,
                            });
                        }
                        else
                        {
                            (change, db) = ParseData(item.Fields, db);
                            if (change)
                            {
                                await _airTableManualSourceRepository.UpdateAsync(db);

                                // Thêm vào queue
                                await _airTableManualSourceWaitingProcessRepository.InsertAsync(new AirTableManualSourceWaitingProcessEntity()
                                {
                                    RecordId = item.Id,
                                    Action = "UPDATE",
                                    ProjectTwitter = db.ProjectTwitter
                                });
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
                    Logger.LogError(ex, LOG_PREFIX + "Sync data from AirTable Manual Source");
                }
            }
        }

        public static (bool, AirTableManualSourceEntity) ParseData(Dictionary<string, object> fields, AirTableManualSourceEntity current)
        {
            bool change = false;

            foreach (var key in fields.Keys)
            {
                var value = fields[key];
                if (value == null)
                {
                    continue;
                }

                switch (key)
                {
                    case "Signals":
                        var signalEle = (JsonElement)value;
                        var signal = (signalEle.EnumerateArray().Select(x => x.ToString())).JoinAsString(",");
                        if (signal != current.Signals)
                        {
                            current.Signals = signal;
                            change = true;
                        }
                        break;
                    case "Type":
                        var type = value.ToString();
                        if (type != current.Type)
                        {
                            current.Type = type;
                            change = true;
                        }
                        break;
                    case "Project Twitter":
                        var projectTwitter = value.ToString();
                        if (projectTwitter != current.ProjectTwitter)
                        {
                            current.ProjectTwitter = projectTwitter;
                            change = true;
                        }
                        break;
                    case "Lastest Signal Date":
                        var signalDate = value.ToString();
                        var signalDateEle = (JsonElement)value;
                        if (signalDateEle.GetDateTime() != current.LastestSignalTime)
                        {
                            current.LastestSignalTime = signalDateEle.GetDateTime();
                        }
                        break;
                    case "Lastest Signal Description":
                        var lastestSignalDesc = value.ToString();
                        if (lastestSignalDesc != current.LastestSignalDescription)
                        {
                            current.LastestSignalDescription = lastestSignalDesc;
                            change = true;
                        }
                        break;
                    case "Lastest Signal From":
                        var lastestSignalFrom = value.ToString();
                        if (lastestSignalFrom != current.LastestSignalFrom)
                        {
                            current.LastestSignalFrom = lastestSignalFrom;
                            change = true;
                        }
                        break;
                    case "Lastest Signal URL":
                        var lastestSignalUrl = value.ToString();
                        if (lastestSignalUrl != current.LastestSignalUrl)
                        {
                            current.LastestSignalUrl = lastestSignalUrl;
                            change = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            return (change, current);
        }
    }
}
