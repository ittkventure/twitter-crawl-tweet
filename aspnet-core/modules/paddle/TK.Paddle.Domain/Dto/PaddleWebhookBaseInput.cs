namespace TK.Paddle.Domain.Dto
{
    public class PaddleWebhookBaseInput
    {
        public long AlertId { get; set; }

        public string AlertName { get; set; }

        public string Raw { get; set; }
    }
}