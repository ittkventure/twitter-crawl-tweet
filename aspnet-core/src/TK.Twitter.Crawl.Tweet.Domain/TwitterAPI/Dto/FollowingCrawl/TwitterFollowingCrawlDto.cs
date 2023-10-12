using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.Twitter.Crawl.Tweet.TwitterAPI.Dto.FollowingCrawl
{
    public class TwitterUserDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ScreenName { get; set; }

        public string Description { get; set; }

        public string TwitterUrl { get; set; }

        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// Thời điểm user được tạo trên twitter
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Thời gian hệ thống phát hiện được user
        /// </summary>
        public DateTime DiscoveredTime { get; set; }
        
        public List<string> Urls { get; set; }

        public bool IsUnavailable { get; set; }

        public string UnavailableReason { get; set; }
    }
}
