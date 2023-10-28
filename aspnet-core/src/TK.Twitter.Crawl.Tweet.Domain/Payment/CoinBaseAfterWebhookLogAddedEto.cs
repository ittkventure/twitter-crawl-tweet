using Volo.Abp.EventBus;

namespace TK.Twitter.Crawl.Tweet
{
    [EventName("CoinBase.Webhook.CoinBaseAfterWebhookLogAddedEto")]
    public class CoinBaseAfterWebhookLogAddedEto
    {
        public string EventId { get; set; }

        public string EventType { get; set; }
    }
}