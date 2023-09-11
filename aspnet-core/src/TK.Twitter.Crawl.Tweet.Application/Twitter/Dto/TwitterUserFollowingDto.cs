using System;

namespace TK.Twitter.Crawl.Twitter.Dto
{
    public class TwitterUserFollowingDto
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

        public DateTime? FollowingTime { get; set; }

        public int AtTimeUserFollowersCount { get; set; }

        public int AtTimeUserTweetCount { get; set; }

        public TwitterUserMetricDto PublicMetric { get; set; }
    }
}
