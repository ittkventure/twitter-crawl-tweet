using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TK.Twitter.Crawl.TwitterAPI.Dto
{
    public class TwitterAPIBaseResponse
    {
        [JsonProperty("errors")]
        public List<TwitterAPIErrorDto> Errors { get; set; }

        public bool TooManyRequest { get; set; }

        public int RateLimit { get; set; }

        public int RateLimitRemaining { get; set; }

        public long RateLimitResetAtTimeStamp { get; set; }

        public DateTime? RateLimitResetAt
        {
            get
            {
                if (RateLimitResetAtTimeStamp <= 0)
                {
                    return null;
                }

                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(RateLimitResetAtTimeStamp);
                return dateTimeOffset.UtcDateTime;
            }
        }
    }
}
