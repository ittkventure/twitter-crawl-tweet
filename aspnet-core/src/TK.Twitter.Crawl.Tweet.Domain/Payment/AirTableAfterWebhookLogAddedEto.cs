using System;
using Volo.Abp.EventBus;

namespace TK.Twitter.Crawl.Tweet
{
    [EventName("AirTable.Webhook.AirTableAfterWebhookLogAddedEto")]
    public class AirTableAfterWebhookLogAddedEto
    {
        public Guid EventId { get; set; }

        public string SystemId { get; set; }

        public string Action { get; set; }
    }
}