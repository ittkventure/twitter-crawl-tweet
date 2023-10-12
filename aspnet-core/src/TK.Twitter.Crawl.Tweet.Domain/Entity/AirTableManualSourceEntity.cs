using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class AirTableManualSourceEntity : FullAuditedEntity<long>
    {
        public string RecordId { get; set; }

        public string ProjectTwitter { get; set; }

        public string Type { get; set; }

        public string Signals { get; set; }

        public string LastestSignalFrom { get; set; }

        public DateTime LastestSignalTime { get; set; }

        public string LastestSignalDescription { get; set; }

        public string LastestSignalUrl { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string UserScreenName { get; set; }
    }
}
