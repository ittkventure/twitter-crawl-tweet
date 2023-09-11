using System;

namespace TK.Twitter.Crawl.Twitter.Dto
{
    public class TwitterUserMetricDto
    {
        public DateTime ScanTime { get; set; }

        /// <summary>
        /// Số người đang follow user hiện tại
        /// </summary>
        public int FollowersCount { get; set; }

        /// <summary>
        /// Lượng người follow user hiện tại thay đổi so với kì trước
        /// </summary>
        public int FollowersCountChange { get; set; }

        /// <summary>
        /// Tổng số tweet đã đăng
        /// </summary>
        public int TweetCount { get; set; }

        /// <summary>
        /// Lượng Tweet thay đổi so với kì trước
        /// </summary>
        public int TweetCountChange { get; set; }

        /// <summary>
        /// Số danh sách theo dõi mà user này đang được add vào
        /// </summary>
        public int ListedCount { get; set; }

        /// <summary>
        /// Số danh sách user xuất hiện thay đổi so với kì trước
        /// </summary>
        public int ListedCountChange { get; set; }

        public int FastFollowersCount { get; set; }

        public int FastFollowersCountChange { get; set; }

        public int FavouritesCount { get; set; }

        public int FavouritesCountChange { get; set; }

        public int FriendsCount { get; set; }

        public int FriendsCountChange { get; set; }

        public int NormalFollowersCount { get; set; }

        public int NormalFollowersCountChange { get; set; }
    }
}
