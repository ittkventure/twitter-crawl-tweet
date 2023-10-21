using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Paddle.Domain.Entity
{
    public class PaddleWebhookProcessEntity : FullAuditedEntity<long>
    {
        public long AlertId { get; set; }

        public string AlertName { get; set; }

        public int ProcessAttempt { get; set; }

        public bool Succeeded { get; set; }

        public bool Ended { get; set; }

        public string Note { get; set; }
    }
}
