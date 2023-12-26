using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class AirTableNoMentionWaitingProcessEntity : FullAuditedEntity<long>
    {
        public string RefId { get; set; }

        public string Action { get; set; }

        public string Signals { get; set; }

        public bool Ended { get; set; }

        public bool Succeed { get; set; }

        public string Note { get; set; }

    }
}
