using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class LeadEntity : FullAuditedEntity<long>
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserScreenName { get; set; }
        public string UserProfileUrl { get; set; }
        public string UserType { get; set; }
        public string UserStatus { get; set; }
        public string Signals { get; set; }
        public string LastestTweetId { get; set; }
        public DateTime? LastestSponsoredDate { get; set; }
        public string LastestSponsoredTweetUrl { get; set; }
        public int? DuplicateUrlCount { get; set; }
        public string TweetDescription { get; set; }
        public string TweetOwnerUserId { get; set; }
        public string MediaMentioned { get; set; }
        public string MediaMentionedProfileUrl { get; set; }
        public int NumberOfSponsoredTweets { get; set; }
        public string SignalDescription { get; set; }
        public string HashTags { get; set; }
    }
}
