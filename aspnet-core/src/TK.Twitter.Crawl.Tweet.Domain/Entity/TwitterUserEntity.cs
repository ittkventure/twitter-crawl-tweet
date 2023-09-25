using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterUserEntity : FullAuditedEntity<long>
    {
        public string UserId { get; set; }

        public string Name { get; set; }

        public string ScreenName { get; set; }

        public string Description { get; set; }

        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// Thời điểm user được tạo trên twitter
        /// </summary>
        public DateTime? CreatedAt { get; set; }
    }
}
