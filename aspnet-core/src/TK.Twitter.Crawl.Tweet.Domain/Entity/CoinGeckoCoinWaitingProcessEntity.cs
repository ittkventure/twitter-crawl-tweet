using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class CoinGeckoCoinWaitingProcessEntity : FullAuditedEntity<long>
    {
        public string CoinId { get; set; }

        public string Action { get; set; }

        public int ProcessAttempt { get; set; }

        public bool Succeed { get; set; }

        public bool Ended { get; set; }

        public string Note { get; set; }
    }
}   
