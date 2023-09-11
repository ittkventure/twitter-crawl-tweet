using System;

namespace TK.Twitter.Crawl.AlphaQuest.Dto
{
    public class AlphaQuestTwitterInfluencerDto
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Name { get; set; }

        public int Tier { get; set; }

        public int Reliability { get; set; }

        public string CreatedUser { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
