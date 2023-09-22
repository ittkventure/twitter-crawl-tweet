using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class LeadWaitingProcessEntity : FullAuditedEntity<long>
    {
        public string BatchKey { get; set; }

        public string UserId { get; set; }

        public string TweetId { get; set; }

        public bool Ended { get; set; }

        public bool Succeed { get; set; }

        public string Note { get; set; }
    }
}
