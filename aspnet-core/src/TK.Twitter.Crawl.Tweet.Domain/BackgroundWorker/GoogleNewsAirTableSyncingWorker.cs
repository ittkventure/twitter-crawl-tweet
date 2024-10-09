using System.Threading.Tasks;
using TK.Twitter.Crawl.Jobs;
using TK.Twitter.Crawl.Tweet.GoogleNews;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.BackgroundWorkers
{
    public class GoogleNewsAirTableSyncingWorker : ITransientDependency
    {
        private readonly IBackgroundJobManager _backgroundJobManager;

        public GoogleNewsAirTableSyncingWorker(IBackgroundJobManager backgroundJobManager)
        {
            _backgroundJobManager = backgroundJobManager;
        }

        [UnitOfWork]
        public async Task DoWorkAsync()
        {
            await _backgroundJobManager.EnqueueAsync(new GoogleNewsAirTableSyncJobArg());
        }
    }
}
