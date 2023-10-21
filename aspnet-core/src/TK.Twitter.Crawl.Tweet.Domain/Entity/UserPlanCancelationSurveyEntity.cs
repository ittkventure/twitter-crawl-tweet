using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class UserPlanCancelationSurveyEntity : FullAuditedEntity<long>
    {
        public Guid UserId { get; set; }

        public string ReasonType { get; set; }

        public string ReasonText { get; set; }

        public string Feedback { get; set; }
    }
}
