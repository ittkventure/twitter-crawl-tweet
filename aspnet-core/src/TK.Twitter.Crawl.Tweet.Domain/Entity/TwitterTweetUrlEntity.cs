using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterTweetUrlEntity : FullAuditedEntity<long>
    {
        public string TweetId { get; set; }

        public string DisplayUrl { get; set; }

        public string ExpandedUrl { get; set; }

        public string Url { get; set; }
    }

}
