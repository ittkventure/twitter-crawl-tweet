using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Tweet.GoogleNews;
using TK.Twitter.Crawl.Tweet.MemoryLock;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Jobs
{
    public class GoogleNewsAirTableSyncJobArg
    {

    }

    [UnitOfWork]
    public class GoogleNewsAirTableSyncJob : AsyncBackgroundJob<GoogleNewsAirTableSyncJobArg>, ITransientDependency
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ILogger<AirTableCheckDataJob> _logger;
        private readonly MemoryLockProvider _memoryLockProvider;
        private readonly GoogleNewsManager _googleNewsManager;

        private const string LOG_PREFIX = "[GoogleNewsAirTableSyncJob] ";

        public GoogleNewsAirTableSyncJob(
            IBackgroundJobManager backgroundJobManager,
            ILogger<AirTableCheckDataJob> logger,
            MemoryLockProvider memoryLockProvider,
            GoogleNewsManager googleNewsManager)
        {
            _backgroundJobManager = backgroundJobManager;
            _logger = logger;
            _memoryLockProvider = memoryLockProvider;
            _googleNewsManager = googleNewsManager;
        }
        public override async Task ExecuteAsync(GoogleNewsAirTableSyncJobArg args)
        {
            using (var handle = _memoryLockProvider.TryAcquireLock($"GoogleNewsAirTableSyncJob"))
            {
                if (handle == null)
                {
                    Logger.LogInformation(LOG_PREFIX + "Some process is running!!!");
                    return;
                }

                try
                {
                    await _googleNewsManager.SyncAirTableAsync();
                }
                catch (BusinessException ex)
                {
                    if (ex.Code == "WaitingProcess:Null")
                    {
                        return;
                    }
                }

                await _backgroundJobManager.EnqueueAsync(new GoogleNewsAirTableSyncJobArg());

            }
        }
    }
}
