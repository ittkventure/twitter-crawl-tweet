﻿using System.ComponentModel.DataAnnotations;
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

        public async Task<string> ExecuteFollowingCrawlJob([Required] string batchKey, [Required] string accountId)
        {
            await _backgroundJobManager.EnqueueAsync(new TwitterTweetCrawlJobArg()
            {
                BatchKey = batchKey,
                TwitterAccountId = accountId
            });

            return "success";
        }
    }
}
