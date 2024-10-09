using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Jobs;
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
    public class AirTableProcessWaitingWorker : ITransientDependency
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<AirTableLeadRecordMappingEntity, long> _airTableLeadRecordMappingRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly Lead3Manager _lead3Manager;
        private readonly AirTableLead3Service _airTableService;

        public AirTableProcessWaitingWorker(
            IBackgroundJobManager backgroundJobManager,
            IRepository<AirTableLeadRecordMappingEntity, long> airTableLeadRecordMappingRepository,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedEventBus distributedEventBus,
            Lead3Manager lead3Manager,
            AirTableLead3Service airTableService
            )
        {
            _backgroundJobManager = backgroundJobManager;
            _airTableLeadRecordMappingRepository = airTableLeadRecordMappingRepository;
            Clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedEventBus = distributedEventBus;
            _lead3Manager = lead3Manager;
            _airTableService = airTableService;
        }

        public IClock Clock { get; }

        [UnitOfWork(IsDisabled = true)]
        public async Task DoWorkAsync()
        {
            using var uow = _unitOfWorkManager.Begin();
            try
            {
                await _backgroundJobManager.EnqueueAsync(new AirTableProcessWaitingJobArg());
                //await uow.SaveChangesAsync();
                await uow.CompleteAsync();
            }
            catch
            {
                await uow.RollbackAsync();
            }
        }
    }
}
