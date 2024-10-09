using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class GoogleNewsAirTableRecordEntity : FullAuditedEntity<long>
    {
        public string RecordId { get; set; }

        public string Source { get; set; }
    }
}
