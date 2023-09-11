using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIGetTweetResponse : TwitterAPIBaseResponse
    {
        public string JsonText { get; set; }
    }

    public class TwitterAPITweetDto
    {
        public string UserId { get; set; }

        public string TweetId { get; set; }

        public int? ViewsCount { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int BookmarkCount { get; set; }

        public int FavoriteCount { get; set; }

        public int QuoteCount { get; set; }

        public int ReplyCount { get; set; }

        public int RetweetCount { get; set; }

        public string UserResultAsJson { get; set; }

        public string EntitiesAsJson { get; set; }

        public string ExtendedEntitiesAsJson { get; set; }

        public string QuoteStatusResultAsJson { get; set; }

        public string FullText { get; set; }

        public bool IsQuoteStatus { get; set; }

        public string Lang { get; set; }

        public string InReplyToScreenName { get; set; }

        public string InReplyToStatusId { get; set; }

        public string InReplyToUserId { get; set; }

        public string ConversationId { get; set; }
    }
}
