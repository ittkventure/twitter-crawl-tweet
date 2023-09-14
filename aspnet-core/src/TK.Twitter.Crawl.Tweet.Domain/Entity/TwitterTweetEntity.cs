using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterTweetEntity : FullAuditedEntity<long>
    {
        public string TweetId { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string UserScreenName { get; set; }

        public string UserScreenNameNormalize { get; set; }

        public int? ViewsCount { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int BookmarkCount { get; set; }

        public int FavoriteCount { get; set; }

        public int QuoteCount { get; set; }

        public int ReplyCount { get; set; }

        public int RetweetCount { get; set; }

        public string FullText { get; set; }

        public string NormalizeFullText { get; set; }

        public bool IsQuoteStatus { get; set; }

        public string Lang { get; set; }

        public string InReplyToScreenName { get; set; }

        public string InReplyToStatusId { get; set; }

        public string InReplyToUserId { get; set; }

        public string ConversationId { get; set; }

    }
}
