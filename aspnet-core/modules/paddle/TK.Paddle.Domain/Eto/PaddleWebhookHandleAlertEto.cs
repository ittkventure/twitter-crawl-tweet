using Volo.Abp.EventBus;

namespace TK.Paddle.Domain.Eto
{
    [EventName("Paddle.Webhook.PaddleWebhookHandleAlertEto")]
    public class PaddleWebhookHandleAlertEto
    {
        public long AlertId { get; set; }

        public string AlertName { get; set; }

        public string Raw { get; set; }
    }
}