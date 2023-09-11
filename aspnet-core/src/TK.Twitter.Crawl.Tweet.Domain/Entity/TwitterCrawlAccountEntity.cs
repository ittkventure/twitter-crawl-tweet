using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterCrawlAccountEntity : FullAuditedEntity<long>
    {
        public string AccountId { get; set; }

        public string CookieCtZeroValue { get; set; }

        public string GuestToken { get; set; }

        public string Cookie { get; set; }

    }
}
