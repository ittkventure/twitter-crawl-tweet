using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterInfluencerEntity : FullAuditedEntity<long>
    {
        public string UserId { get; set; }

        public string ScreenName { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Thuộc tính này để đánh dấu influencer này không được xóa
        /// </summary>
        public bool IsProtected { get; set; }

    }
}
