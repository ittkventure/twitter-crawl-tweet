using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class GoogleNewsRecordEntity : FullAuditedEntity<long>
    {
        public string Link { get; set; }

        public string Title { get; set; }

        public string Source { get; set; }

        public string Date { get; set; }

        public DateTime DateValue { get; set; }

        public string Snippet { get; set; }

        public string Thumbnail { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
