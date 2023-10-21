using System;

namespace TK.Paddle.Domain.Dto
{
    public class PaymentOrderPassthroughtDto
    {
        public Guid OrderId { get; set; }

        public Guid UserId { get; set; }
    }
}