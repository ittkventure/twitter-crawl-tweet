using System;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.Twitter
{
    public class TweetDto
    {
        public string TweetId { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string UserScreenName { get; set; }

        public int? ViewsCount { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int BookmarkCount { get; set; }

        public int FavoriteCount { get; set; }

        public int QuoteCount { get; set; }

        public int ReplyCount { get; set; }

        public int RetweetCount { get; set; }

        public string FullText { get; set; }

        public bool IsQuoteStatus { get; set; }

        public string Lang { get; set; }

        public string InReplyToScreenName { get; set; }

        public string InReplyToStatusId { get; set; }

        public string InReplyToUserId { get; set; }

        public string ConversationId { get; set; }

        public List<MediaDto> Medias { get; set; }
        public List<UserMentionDto> UserMentions { get; set; }

        public List<string> HashTags { get; set; }
        public List<string> Symbols { get; set; }
        public List<string> Urls { get; set; }

        public class MediaDto
        {
            public string DisplayUrl { get; set; }

            public string ExpandedUrl { get; set; }

            public string Url { get; set; }

            public string MediaId { get; set; }

            public string MediaUrlHttps { get; set; }

            public string Type { get; set; }
        }

        public class UserMentionDto
        {
            public string UserId { get; set; }

            public string Name { get; set; }

            public string ScreenName { get; set; }
        }
    }
}
