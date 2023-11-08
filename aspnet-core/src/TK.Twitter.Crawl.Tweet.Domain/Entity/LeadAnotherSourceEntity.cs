using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class LeadAnotherSourceEntity : FullAuditedEntity<long>
    {
        public string RefId { get; set; }

        public string Source { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string UserScreenName { get; set; }

        public string SignalUrl { get; set; }

        public string Signals { get; set; }

        public string Description { get; set; }

        public string MediaMentioned { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
