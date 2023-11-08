using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class CoinGeckoCoinEntity : FullAuditedEntity<long>
    {
        public string CoinId { get; set; }

        public string Symbol { get; set; }

        public string Name { get; set; }

        public string JsonContent { get; set; }
    }
}
