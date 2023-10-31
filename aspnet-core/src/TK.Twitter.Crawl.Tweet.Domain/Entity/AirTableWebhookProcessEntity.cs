using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class AirTableWebhookProcessEntity : FullAuditedEntity<long>
    {
        public Guid EventId { get; set; }

        public string SystemId { get; set; }

        public string Action { get; set; }

        public int ProcessAttempt { get; set; }

        public bool Succeeded { get; set; }

        public bool Ended { get; set; }

        public string Note { get; set; }
    }
}
