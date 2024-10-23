using System.Threading.Tasks;
using TK.Twitter.Crawl.Tweet.GoogleNews;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.BackgroundWorkers
{
    public class GoogleNewsPullingCryptoCurrencyTopicWorker : ITransientDependency
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly GoogleNewsManager _googleNewsManager;

        public GoogleNewsPullingCryptoCurrencyTopicWorker(GoogleNewsManager googleNewsManager)
        {
            _googleNewsManager = googleNewsManager;
        }

        [UnitOfWork]
        public async Task DoWorkAsync()
        {
            await _googleNewsManager.CrawlCryptoCurrencyTopicAsync();
        }
    }
}
