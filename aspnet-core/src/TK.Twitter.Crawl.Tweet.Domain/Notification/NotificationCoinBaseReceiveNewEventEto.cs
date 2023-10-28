using Volo.Abp.EventBus;

namespace TK.Twitter.Crawl.Notification
{
    [EventName("Notification.CoinBaseReceiveNewEvent")]
    public class NotificationCoinBaseReceiveNewEventEto
    {
        public string EventId { get; set; }

        public string EventType { get; set; }
    }
}
