using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class AirTableManualSourceWaitingProcessEntity : FullAuditedEntity<long>
    {
        public string RecordId { get; set; }

        public string ProjectTwitter { get; set; }


        public string Action { get; set; }

        public bool Ended { get; set; }

        public bool Succeed { get; set; }

        public string Note { get; set; }

    }
}
