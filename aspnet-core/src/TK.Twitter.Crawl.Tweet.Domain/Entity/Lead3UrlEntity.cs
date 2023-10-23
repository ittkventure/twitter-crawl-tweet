using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class Lead3UrlEntity : FullAuditedEntity<long>
    {
        public string Url { get; set; }

        public string Type { get; set; }
    }
}
