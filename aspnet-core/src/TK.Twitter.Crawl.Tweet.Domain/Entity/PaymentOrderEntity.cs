using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    public class PaymentOrderEntity : FullAuditedAggregateRoot<Guid>
    {
        public PaymentOrderEntity()
        {

        }

        public PaymentOrderEntity(Guid id) : base(id)
        {

        }

        public Guid OrderId { get; set; }

        public Guid? UserId { get; set; }

        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }

        public int OrderStatusId { get; set; }

        public int PaymentMethod { get; set; }

        public PaymentOrderPayLinkEntity Paylink { get; set; }

        public PaymentOrderPaddleEntity PaddlePaymentInfo { get; set; }

        public void AddPayLink(string url)
        {
            Paylink = new PaymentOrderPayLinkEntity()
            {
                OrderId = OrderId,
                GeneratedAt = CreatedAt,
                PayLink = url
            };
        }

        public void AddPaddlePaymentInfo(PaymentOrderPaddleEntity entity)
        {
            entity.OrderId = OrderId;
            PaddlePaymentInfo = entity;
        }
    }
}
