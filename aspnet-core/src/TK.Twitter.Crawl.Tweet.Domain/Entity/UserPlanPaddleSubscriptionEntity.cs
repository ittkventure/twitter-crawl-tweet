using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class UserPlanPaddleSubscriptionEntity : FullAuditedEntity<long>
    {
        public Guid UserId { get; set; }

        public long SubscriptionId { get; set; }

        public bool IsCanceled { get; set; }

        public DateTime? CancellationEffectiveDate { get; set; }

        public bool IsCurrent { get; set; }

    }
}
