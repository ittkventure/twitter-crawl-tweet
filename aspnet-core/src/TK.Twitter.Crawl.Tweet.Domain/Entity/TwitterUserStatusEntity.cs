using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterUserStatusEntity : FullAuditedEntity<long>
    {
        public string UserId { get; set; }

        public string Status { get; set; }

        public bool IsUserSuppliedValue { get; set; }
    }

}
