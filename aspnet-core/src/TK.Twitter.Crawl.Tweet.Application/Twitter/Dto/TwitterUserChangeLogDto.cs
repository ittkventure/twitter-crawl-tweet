using System;

namespace TK.Twitter.Crawl.Twitter.Dto
{
    public class TwitterUserChangeLogDto
    {
        public DateTime DataTime { get; set; }

        public string DataType { get; set; }

        public string OriginalData { get; set; }

        public string NewValue { get; set; }
    }
}
