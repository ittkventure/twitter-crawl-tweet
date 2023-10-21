using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class EmailLogEntity : AuditedEntity<long>
    {
        public string To { get; set; }

        public string Bcc { get; set; }

        public string Cc { get; set; }

        public string Body { get; set; }

        public bool IsBodyHtml { get; set; }

        public string Subject { get; set; }

        public int ProcessAttempt { get; set; }

        public bool Ended { get; set; }

        public bool Succeeded { get; set; }

        public string Note { get; set; }
    }
}
