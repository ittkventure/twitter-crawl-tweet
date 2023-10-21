using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    /// <summary>
    /// Lịch sử upgrade plan của user
    /// </summary>
    public class UserPlanUpgradeHistoryEntity : FullAuditedEntity<long>
    {
        public Guid UserId { get; set; }

        public string Type { get; set; }

        public string OldPlanKey { get; set; }

        public string NewPlanKey { get; set; }

        public DateTime CreatedAt { get; set; }

        public string TimeAddedType { get; set; }

        public int TimeAdded { get; set; }

        public DateTime NewExpiredTime { get; set; }

        public string Reference { get; set; }
    }
}
