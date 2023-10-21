using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class PaymentOrderPayLinkEntity : FullAuditedEntity<long>
    {
        public Guid OrderId { get; set; }

        public string PayLink { get; set; }

        public DateTime GeneratedAt { get; set; }
    }
}
