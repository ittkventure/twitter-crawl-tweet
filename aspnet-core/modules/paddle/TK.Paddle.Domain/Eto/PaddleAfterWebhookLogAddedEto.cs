using Volo.Abp.EventBus;

namespace TK.Paddle.Domain.Eto
{
    /// <summary>
    /// ETO của webhook. Implement Handler với ETO này ở các module khác để thực hiện các đoạn logic code khác nhau. 
    /// 1 ETO có thể có nhiều handler
    /// </summary>
    [EventName("Paddle.Webhook.PaddleAfterWebhookLogAddedEto")]
    public class PaddleAfterWebhookLogAddedEto
    {
        public long AlertId { get; set; }

        public string AlertName { get; set; }
    }
}
