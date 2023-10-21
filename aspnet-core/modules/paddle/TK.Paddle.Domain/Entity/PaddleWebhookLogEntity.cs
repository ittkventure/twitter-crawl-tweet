using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Paddle.Domain.Entity
{
    public class PaddleWebhookLogEntity : FullAuditedEntity<Guid>
    {
        public long AlertId { get; set; }

        public string AlertName { get; set; }

        public string Raw { get; set; }
    }
}
