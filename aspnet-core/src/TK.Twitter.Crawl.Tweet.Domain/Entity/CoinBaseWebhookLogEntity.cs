using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class CoinBaseWebhookLogEntity : FullAuditedEntity<Guid>
    {
        public string EventId { get; set; }

        public string EventType { get; set; }

        public string Raw { get; set; }
    }
}
