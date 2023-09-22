using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class AirTableWaitingProcessEntity : FullAuditedEntity<long>
    {
        public string BatchKey { get; set; }

        public string UserId { get; set; }

        public string TweetId { get; set; }

        public string Action { get; set; }

        public bool Ended { get; set; }

        public bool Succeed { get; set; }

        public string Note { get; set; }

        public long LeadId { get; set; }

        public string UserScreenName { get; set; }
    }
}
