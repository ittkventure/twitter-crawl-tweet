using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterUserTypeEntity : FullAuditedEntity<long>
    {
        public string UserId { get; set; }

        public string Type { get; set; }
    }

}
