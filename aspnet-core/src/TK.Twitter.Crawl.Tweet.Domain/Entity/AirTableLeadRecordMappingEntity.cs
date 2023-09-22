using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class AirTableLeadRecordMappingEntity : FullAuditedEntity<long>
    {
        public string AirTableRecordId { get; set; }

        public string ProjectUserId { get; set; }
    }

}
