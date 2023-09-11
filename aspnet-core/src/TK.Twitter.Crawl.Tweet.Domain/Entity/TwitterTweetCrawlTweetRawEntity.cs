using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterTweetCrawlRawEntity : FullAuditedEntity<long>
    {
        public string TweetId { get; set; }

        public string JsonContent { get; set; }
    }
}
