using System;

namespace TK.Twitter.Crawl.Entity.Dapper
{
    public class TwitterTweetCrawlTweetDapperEntity
    {
        /// <summary>
        /// User ID của KOL (Người đi follow dự án)
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Twitter UserID của người được follow
        /// </summary>
        public string FollowingUserId { get; set; }

        /// <summary>
        /// Twitter Name của người được follow
        /// </summary>
        public string FollowingUserName { get; set; }

        /// <summary>
        /// Twitter Description của người được follow
        /// </summary>
        public string FollowingUserDescription { get; set; }

        /// <summary>
        /// Twitter ScreenName của người được follow
        /// </summary>
        public string FollowingUserScreenName { get; set; }

        /// <summary>
        /// Twitter ProfileImageUrl của người được follow
        /// </summary>
        public string FollowingUserProfileImageUrl { get; set; }

        /// <summary>
        /// Thời điểm user được tạo
        /// </summary>
        public DateTime? FollowingUserCreatedAt { get; set; }

        /// <summary>
        /// Số người đang follow user hiện tại
        /// </summary>
        public int FollowingUserFollowersCount { get; set; }

        /// <summary>
        /// Tổng số tweet đã đăng
        /// </summary>
        public int FollowingUserStatusesCount { get; set; }

        /// <summary>
        /// Số danh sách theo dõi mà user này đang được add vào
        /// </summary>
        public int FollowingUserListedCount { get; set; }

        public int FollowingUserFastFollowersCount { get; set; }

        public int FollowingUserFavouritesCount { get; set; }

        /// <summary>
        /// Số người following
        /// </summary>
        public int FollowingUserFriendsCount { get; set; }

        public int FollowingUserNormalFollowersCount { get; set; }

        public string EntitiesAsJson { get; set; }

        public string ProfessionalAsJson { get; set; }

        public DateTime DiscoveredTime { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
