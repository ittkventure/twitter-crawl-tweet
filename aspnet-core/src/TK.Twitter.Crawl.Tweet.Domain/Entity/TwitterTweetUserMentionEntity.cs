using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterTweetUserMentionEntity : FullAuditedEntity<long>
    {
        public string TweetId { get; set; }

        public string UserId { get; set; }

        public string Name { get; set; }

        public string ScreenName { get; set; }

        public string NormalizeScreenName { get; set; }

        public string NormalizeName { get; set; }
    }
}
