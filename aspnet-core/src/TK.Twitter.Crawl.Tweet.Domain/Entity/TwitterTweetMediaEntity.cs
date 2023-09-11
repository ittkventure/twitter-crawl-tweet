using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterTweetMediaEntity : FullAuditedEntity<long>
    {
        public string TweetId { get; set; }

        public string DisplayUrl { get; set; }

        public string ExpandedUrl { get; set; }

        public string Url { get; set; }

        public string MediaId { get; set; }

        public string MediaUrlHttps { get; set; }

        public string Type { get; set; }
    }
}
