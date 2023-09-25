using System.Threading.Tasks;
using TK.Twitter.Crawl.Jobs;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.BackgroundWorkers
{
    public class AirTableCheckDataWorker : ITransientDependency
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public AirTableCheckDataWorker(
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
                await _backgroundJobManager.EnqueueAsync(new AirTableCheckDataJobArg());
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
