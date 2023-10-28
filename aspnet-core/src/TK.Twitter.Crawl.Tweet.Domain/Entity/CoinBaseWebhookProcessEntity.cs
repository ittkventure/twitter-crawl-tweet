using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class CoinBaseWebhookProcessEntity : FullAuditedEntity<long>
    {
        public string EventId { get; set; }

        public string EventType { get; set; }

        public int ProcessAttempt { get; set; }

        public bool Succeeded { get; set; }

        public bool Ended { get; set; }

        public string Note { get; set; }
    }
}
