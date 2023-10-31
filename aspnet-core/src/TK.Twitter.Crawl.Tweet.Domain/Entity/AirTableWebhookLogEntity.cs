using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class AirTableWebhookLogEntity : FullAuditedEntity<Guid>
    {
        public Guid EventId { get; set; }

        public string SystemId { get; set; }

        public string Action { get; set; }

        public string Raw { get; set; }
    }
}
