using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using TK.Paddle.Domain;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet.User;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using static TK.Twitter.Crawl.CrawlConsts;

namespace TK.Twitter.Crawl.Tweet.Payment
{
    public class PaymentAppService : CrawlAppService
    {
        private readonly IRepository<PaymentOrderEntity, Guid> _paymentOrderRepository;
        private readonly PaddlePaymentManager _paddlePaymentManager;
        private readonly UserPlanManager _userPlanManager;
        private readonly IConfiguration _configuration;

        public PaymentAppService(
            IRepository<PaymentOrderEntity, Guid> paymentOrderRepository,
            PaddlePaymentManager paddlePaymentManager,
            UserPlanManager userPlanManager,
            IConfiguration configuration)
        {
            _paymentOrderRepository = paymentOrderRepository;
            _paddlePaymentManager = paddlePaymentManager;
            _userPlanManager = userPlanManager;
            _configuration = configuration;
        }

#if DEBUG
        public async Task<string> SubscribeAsync(string planKey = "PremiumAnnually")
#else
        public async Task<string> SubscribeAsync(string planKey)
#endif
        {
            if (!CrawlConsts.Payment.PAID_PLAN.Contains(planKey))
            {
                throw new BusinessException(CrawlDomainErrorCodes.NotFound, "Plan Key not found");
            }

            var now = Clock.Now;
            var order = await _paymentOrderRepository.InsertAsync(new PaymentOrderEntity()
            {
                OrderId = GuidGenerator.Create(),
                CreatedAt = now,
                OrderStatusId = (int)PaymentOrderStatus.Created,
                PaymentMethod = (int)PaymentMethod.Paddle
            });

            var planId = CrawlConsts.Paddle.GetPaddlePlanId(planKey, _configuration);
            string payLink = await _paddlePaymentManager.GeneratePaylink(order.OrderId, planId);

            order.AddPayLink(payLink);

            return payLink;
        }

        public CoinBase.CoinBasePlanConfig GetCoinBaseCheckOutId(string planKey)
        {
            var plans = CrawlConsts.CoinBase.GetPlans(_configuration);
            return plans.FirstOrDefault(x => x.Key == planKey);
        }

        public async Task<bool> CheckOrderPaymentStatus(Guid id)
        {
            //// Chỉ nhưng user đã confirm email thì mới được thực hiện
            //if (!await IsUserEmailConfirmed())
            //{
            //    throw new BusinessException(AlphaQuestDomainErrorCodes.UserEmailNotConfirmed);
            //}

            //CheckLogin();

            var order = await _paymentOrderRepository.FirstOrDefaultAsync(x => x.OrderId == id);
            if (order == null)
            {
                throw new BusinessException(CrawlDomainErrorCodes.PaymentOrderNotFound, "Order not found");
            }

            //if (order.UserId != CurrentUser.Id)
            //{
            //    throw new BusinessException(CrawlDomainErrorCodes.PaymentOrderNotBelongToUser, "Order not belong to user");
            //}

            return order.OrderStatusId == (int)PaymentOrderStatus.Completed;
        }
    }
}
