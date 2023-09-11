using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterTweetSymbolEntity : FullAuditedEntity<long>
    {
        public string TweetId { get; set; }

        public string SymbolText { get; set; }
    }

}
