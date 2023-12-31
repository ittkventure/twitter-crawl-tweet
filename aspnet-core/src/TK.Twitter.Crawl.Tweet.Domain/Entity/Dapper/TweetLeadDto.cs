﻿using System;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.Entity.Dapper
{
    public class TweetLeadDto
    {
        public string UserId { get; set; }
        public string LastestTweetId { get; set; }
        public string UserName { get; set; }
        public string UserScreenName { get; set; }
        public string UserType { get; set; }
        public string UserStatus { get; set; }
        public DateTime? LastestSponsoredDate { get; set; }
        public string LastestSponsoredTweetUrl { get; set; }
        public int? DuplicateUrlCount { get; set; }
        public string TweetDescription { get; set; }
        public string NormalizeTweetDescription { get; set; }
        public string TweetOwnerUserId { get; set; }
        public string MediaMentioned { get; set; }
        public int NumberOfSponsoredTweets { get; set; }
        public List<string> HashTags { get; set; }
        public List<string> Signals { get; set; }
        public string SignalDescription { get; set; }
    }
}
