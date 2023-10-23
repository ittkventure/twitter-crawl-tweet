using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet.User;
using Volo.Abp.Domain.Repositories;

namespace TK.Twitter.Crawl.Tweet.Lead3
{
#if !DEBUG
    [Authorize]
#endif
    public class Lead3AppService : CrawlAppService
    {
        private readonly UserPlanManager _userPlanManager;
        private readonly IRepository<Lead3UrlEntity, long> _lead3UrlRepository;

        public Lead3AppService(UserPlanManager userPlanManager,
            IRepository<Lead3UrlEntity, long> lead3UrlRepository)
        {
            _userPlanManager = userPlanManager;
            _lead3UrlRepository = lead3UrlRepository;
        }

        public async Task<Lead3EmbedUrlDto> GetEmbedUrlAsync()
        {
            CheckLogin();

            var dto = new Lead3EmbedUrlDto()
            {
                PlanKey = CrawlConsts.Payment.FREE
            };

            var (hasPaid, expiredTime) = await _userPlanManager.CheckPaidPlan(CurrentUser.Email);
            if (!hasPaid)
            {
                return dto;
            }

            var currentPlan = await _userPlanManager.GetCurrentPlan(CurrentUser.Id.Value);
            dto.PlanKey = currentPlan.PlanKey;

            var urls = await _lead3UrlRepository.GetListAsync();

            if (CrawlConsts.Payment.IsPremiumPlan(currentPlan.PlanKey))
            {
                dto.IsPremiumPlan = true;
                dto.Url1 = urls.FirstOrDefault(x => x.Type == "PREMIUM")?.Url;
            }
            else if (CrawlConsts.Payment.IsStandardPlan(currentPlan.PlanKey))
            {
                dto.IsStandardPlan = true;
                dto.Url1 = urls.FirstOrDefault(x => x.Type == "STANDARD_THIS_MONTH").Url;
                dto.Url2 = urls.FirstOrDefault(x => x.Type == "STANDARD_LAST_MONTH").Url;
            }

            return dto;
        }
    }

    public class Lead3EmbedUrlDto
    {
        public string PlanKey { get; set; }

        public bool IsPremiumPlan { get; set; }

        public bool IsStandardPlan { get; set; }

        public string Url1 { get; set; }

        public string Url2 { get; set; }
    }
}
