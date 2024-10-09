using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class GoogleNewsWaitingProcessEntity : FullAuditedEntity<long>
    {
        public string SourceName { get; set; }
        public int StatusId { get; set; }
        public int Attempt { get; set; }
        public string Note { get; set; }
    }
}
