using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterUserSignalEntity : FullAuditedEntity<long>
    {
        public string UserId { get; set; }

        public string TweetId { get; set; }

        public string Signal { get; set; }
    }
}
