using System;

namespace TK.Twitter.Crawl.Tweet.Admin.Dto
{
    public class SubcriberDto
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }

        public string Plan { get; set; }

        public DateTime? SubscribedEndDate { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
