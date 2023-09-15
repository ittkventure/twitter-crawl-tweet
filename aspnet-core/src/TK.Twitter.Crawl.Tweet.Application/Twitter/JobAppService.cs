using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Jobs;
using Volo.Abp.BackgroundJobs;

namespace TK.Twitter.Crawl.Twitter
{
    public class JobAppService : CrawlAppService
    {
        private readonly IBackgroundJobManager _backgroundJobManager;

        public JobAppService(IBackgroundJobManager backgroundJobManager)
        {
            _backgroundJobManager = backgroundJobManager;
        }

        public async Task<string> ExecuteTweetCrawlJob([Required] string batchKey, [Required] string accountId)
        {
            await _backgroundJobManager.EnqueueAsync(new TwitterTweetCrawlJobArg()
            {
                BatchKey = batchKey,
                TwitterAccountId = accountId
            });

            return "success";
        }

        public async Task<string> ExecuteTweetSingleCrawlJob([Required] string userId, [Required] string accountId)
        {
            await _backgroundJobManager.EnqueueAsync(new TwitterTweetSingleUserCrawlJobArg()
            {
                TwitterAccountId = accountId,
                UserId = userId
            });

            return "success";
        }
    }
}
