using Medallion.Threading;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using TK.Paddle.Domain;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet.User;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace TK.Twitter.Crawl.Tweet.Payment
{
    public class PaymentAppService : CrawlAppService
    {
        private readonly IDistributedLockProvider _distributedLockProvider;
        private readonly IRepository<PaymentOrderEntity, Guid> _paymentOrderRepository;
        private readonly PaddlePaymentManager _paddlePaymentManager;
        private readonly UserPlanManager _userPlanManager;
        private readonly IConfiguration _configuration;

        public PaymentAppService(
            IDistributedLockProvider distributedLockProvider,
            IRepository<PaymentOrderEntity, Guid> paymentOrderRepository,
            PaddlePaymentManager paddlePaymentManager,
            UserPlanManager userPlanManager,
            IConfiguration configuration)
        {
            _distributedLockProvider = distributedLockProvider;
            _paymentOrderRepository = paymentOrderRepository;
            _paddlePaymentManager = paddlePaymentManager;
            _userPlanManager = userPlanManager;
            _configuration = configuration;
        }

#if DEBUG
        public async Task<string> SubscribeAsync(string email = "hoangphihai93@gmail.com", string planKey = "PremiumAnnually")
#else
        public async Task<string> SubscribeAsync(string email, string planKey)
#endif
        {
            if (!CrawlConsts.Payment.CheckValid(planKey))
            {
                throw new BusinessException(CrawlDomainErrorCodes.NotFound, "Plan Key not found");
            }

            var @lock = _distributedLockProvider.CreateLock($"__GetPayLink__{email}");
            using (var handle = await @lock.TryAcquireAsync())
            {
                if (handle == null)
                {
                    throw new BusinessException(CrawlDomainErrorCodes.AnOnGoingProcessHasNotBeenCompleted);
                }

                var (hasPremium, _) = await _userPlanManager.CheckPaidPlan(email);
                if (hasPremium)
                {
                    throw new BusinessException(CrawlDomainErrorCodes.UserPlanPremiumPlanAlreadyExisted, "User is on premium plan");
                }

                var now = Clock.Now;
                var order = await _paymentOrderRepository.InsertAsync(new PaymentOrderEntity()
                {
                    Email = email,
                    OrderId = GuidGenerator.Create(),
                    CreatedAt = now,
                    OrderStatusId = (int)PaymentOrderStatus.Created,
                    PaymentMethod = (int)PaymentMethod.Paddle
                });

                var planId = CrawlConsts.Payment.GetPlanId(planKey, _configuration);
                string payLink = await _paddlePaymentManager.GeneratePaylink(order.OrderId, email, planId);

                order.AddPayLink(payLink);

                return payLink;
            }
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
