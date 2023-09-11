using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class TwitterTweetCrawlBatchEntity : FullAuditedAggregateRoot<long>
    {
        public TwitterTweetCrawlBatchEntity()
        {
            QueueItems = new List<TwitterTweetCrawlQueueEntity>();
        }

        public TwitterTweetCrawlBatchEntity(long id, string key) : base(id)
        {
            Check.NotNull(key, nameof(key));
            Id = id;
            Key = key;
            QueueItems = new List<TwitterTweetCrawlQueueEntity>();
        }

        /// <summary>
        /// Lưu thông tin thời gian mỗi thời điểm thực hiện crawl
        /// </summary>
        public DateTime CrawlTime { get; set; }

        public string Key { get; set; }
        
        public List<TwitterTweetCrawlQueueEntity> QueueItems { get; set; }

        public void AddQueueItem(TwitterTweetCrawlQueueEntity item)
        {
            if (QueueItems.IsEmpty())
            {
                QueueItems.Add(item);
            }
            else
            {
                if (!QueueItems.Any(i => i.UserId == item.UserId))
                {
                    QueueItems.Add(item);
                }
            }
        }


        public const string KEY_FORMAT = "yyyyMMddHHmmss";
    }
}
