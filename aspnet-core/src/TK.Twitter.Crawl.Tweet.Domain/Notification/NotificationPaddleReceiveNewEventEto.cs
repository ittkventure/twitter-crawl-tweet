using Volo.Abp.EventBus;

namespace TK.Twitter.Crawl.Notification
{
    [EventName("Notification.PaddleReceiveNewEvent")]
    public class NotificationPaddleReceiveNewEventEto
    {
        public long AlertId { get; set; }

        public string AlertName { get; set; }
    }
}
