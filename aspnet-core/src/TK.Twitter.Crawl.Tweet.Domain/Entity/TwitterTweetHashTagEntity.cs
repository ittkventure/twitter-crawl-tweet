using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterTweetHashTagEntity : FullAuditedEntity<long>
    {
        public string TweetId { get; set; }

        public string Text { get; set; }
    }

}
