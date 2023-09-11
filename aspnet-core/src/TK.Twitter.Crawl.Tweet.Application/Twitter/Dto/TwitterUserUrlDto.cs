using System.Collections.Generic;

namespace TK.Twitter.Crawl.Twitter.Dto
{
    public class TwitterUserUrlDto
    {
        public string UserId { get; set; }

        public List<string> Urls { get; set; }
    }
}
