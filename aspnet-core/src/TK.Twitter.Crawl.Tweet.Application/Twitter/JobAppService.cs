using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Jobs;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;

namespace TK.Twitter.Crawl.Twitter
{
#if !DEBUG
    [Microsoft.AspNetCore.Authorization.Authorize]
#endif
    public class JobAppService : CrawlAppService
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<LeadEntity, long> _leadRepository;

        public JobAppService(IBackgroundJobManager backgroundJobManager, IRepository<LeadEntity, long> leadRepository)
        {
            _backgroundJobManager = backgroundJobManager;
            _leadRepository = leadRepository;
        }

        public async Task<string> ExecuteAddUserJob()
        {
            var leads = await AsyncExecuter.ToListAsync(
                        from l in await _leadRepository.GetQueryableAsync()
                        select l.UserId
                );

            await _backgroundJobManager.EnqueueAsync(new TwitterAddUserJobArg()
            {
                UserIds = leads,
            });

            return "success";
        }
    }
}
