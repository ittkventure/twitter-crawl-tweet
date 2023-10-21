using System;

namespace TK.Twitter.Crawl.Tweet.Payment.Dto.CacheItem
{
    public class AccountVerifyTokenCacheItem
    {
        public string Token { get; set; }

        public Guid UserId { get; set; }
    }
}
