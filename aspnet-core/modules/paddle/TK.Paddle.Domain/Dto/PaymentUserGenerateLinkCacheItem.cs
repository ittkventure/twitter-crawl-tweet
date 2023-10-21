using System;

namespace TK.Paddle.Domain.Dto
{
    public class PaymentUserGenerateLinkCacheItem
    {
        public Guid OrderId { get; set; }

        public DateTime LastGenerateTime { get; set; }

        public string PayLink { get; set; }
    }
}
