using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;

namespace TK.Twitter.Crawl.Tweet.User
{
    public class UserPlanManager : DomainService
    {
        private const int PADDING_HOURS = 12;
        private const int TRIAL_DAY_COUNT = 7;

        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly IRepository<UserPlanEntity, Guid> _userPlanRepository;
        private readonly IRepository<UserPlanPaddleSubscriptionEntity, long> _userPlanPaddleSubscriptionRepository;
        private readonly IRepository<UserPlanUpgradeHistoryEntity, long> _userPlanUpgradeHistoryRepository;
        private readonly IRepository<UserPlanCancelationSurveyEntity, long> _userPlanCancelationSurveyRepository;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public UserPlanManager(
            IRepository<IdentityUser, Guid> userRepository,
            IRepository<UserPlanEntity, Guid> userPlanRepository,
            IRepository<UserPlanPaddleSubscriptionEntity, long> userPlanPaddleSubscriptionRepository,
            IRepository<UserPlanUpgradeHistoryEntity, long> userPlanUpgradeHistoryRepository,
            IRepository<UserPlanCancelationSurveyEntity, long> userPlanCancelationSurveyRepository,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _userPlanRepository = userPlanRepository;
            _userPlanPaddleSubscriptionRepository = userPlanPaddleSubscriptionRepository;
            _userPlanUpgradeHistoryRepository = userPlanUpgradeHistoryRepository;
            _userPlanCancelationSurveyRepository = userPlanCancelationSurveyRepository;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task AddDefaultAsync(Guid userId)
        {
            if (!await _userPlanRepository.AnyAsync(x => x.Id == userId))
            {
                // Hàm này chủ yếu được sử dụng khi user vừa mới đăng ký
                // Nên k cần thực hiện thêm history
                await _userPlanRepository.InsertAsync(new UserPlanEntity()
                {
                    UserId = userId,
                    PlanKey = CrawlConsts.Payment.FREE,
                    CreatedAt = Clock.Now,
                    ExpiredAt = DateTime.MaxValue,
                });
            }
        }

        /// <summary>
        /// Kiểm tra user có thuộc tài khoản premium k?
        /// Nếu đã có premium nhưng hết hạn thì KQ trả về false kèm thời gian hết hạn
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<(bool, DateTime?)> CheckPremiumPlan(string email)
        {
            bool hasPremium = false;
            DateTime? expiredDate = null;

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                return (hasPremium, expiredDate);
            }

            var query = from q in await _userPlanRepository.GetQueryableAsync()
                        where q.UserId == user.Id && CrawlConsts.Payment.PAID_PLAN.Contains(q.PlanKey)
                        select new { q.UserId, q.ExpiredAt };

            var currentPlan = await AsyncExecuter.FirstOrDefaultAsync(query);
            if (currentPlan == null)
            {
                return (hasPremium, expiredDate);
            }

            hasPremium = true;
            expiredDate = currentPlan.ExpiredAt;

            // Nếu đã hết hạn thì cũng trả về luôn là không có Premium
            if (IsExpired(expiredDate.Value))
            {
                hasPremium = false;
            }

            return (hasPremium, expiredDate);
        }

        public async Task<UserPlanEntity> UpgradeOrRenewalPlan(Guid userId, string planKey, int monthCount = 1, string historyRef = null)
        {
            if (monthCount <= 0)
            {
                throw new ArgumentException(nameof(monthCount));
            }

            var hasUser = await _userRepository.AnyAsync(x => x.Id == userId);
            if (!hasUser)
            {
                throw new BusinessException(CrawlDomainErrorCodes.NotFound, "User not found");
            }

            string oldPlan = string.Empty;

            var currentPlanQuery = await _userPlanRepository.WithDetailsAsync(x => x.UpgradeHistoryItems, x => x.PaddleSubscriptions);
            currentPlanQuery = currentPlanQuery.Where(x => x.UserId == userId);

            bool sendEmailWelcome = false;

            var now = Clock.Now;

            var expiredAt = CrawlConsts.Payment.GetPlanExpiredAt(planKey, now, PADDING_HOURS, _configuration);

            var currentPlan = await AsyncExecuter.FirstOrDefaultAsync(currentPlanQuery);
            if (currentPlan == null)
            {
                currentPlan = new UserPlanEntity()
                {
                    UserId = userId,
                    PlanKey = planKey,
                    CreatedAt = now,
                    ExpiredAt = expiredAt
                };                
                currentPlan = await _userPlanRepository.InsertAsync(currentPlan, autoSave: true);

                sendEmailWelcome = true;
            }
            else
            {
                oldPlan = currentPlan.PlanKey;
                sendEmailWelcome = oldPlan == CrawlConsts.Payment.FREE;

                currentPlan.PlanKey = planKey;
                currentPlan.ExpiredAt = Clock.Now.AddMonths(monthCount).AddHours(PADDING_HOURS);
                currentPlan = await _userPlanRepository.UpdateAsync(currentPlan);
            }

            currentPlan.AddHistory(new UserPlanUpgradeHistoryEntity()
            {
                UserId = userId,
                Type = CrawlConsts.Payment.Type.UPGRADE_OR_RENEWAL,
                OldPlanKey = oldPlan,
                NewPlanKey = currentPlan.PlanKey,
                CreatedAt = Clock.Now,
                TimeAddedType = "MONTH",
                TimeAdded = monthCount,
                NewExpiredTime = currentPlan.ExpiredAt,
                Reference = historyRef
            });

            currentPlan = await _userPlanRepository.UpdateAsync(currentPlan);

            return currentPlan;
        }

        public async Task<UserPlanEntity> GetCurrentPlan(Guid userId)
        {
            var hasUser = await _userRepository.AnyAsync(x => x.Id == userId);
            if (!hasUser)
            {
                throw new BusinessException(CrawlDomainErrorCodes.NotFound, "User not found");
            }

            var currentPlanQuery = await _userPlanRepository.WithDetailsAsync(x => x.UpgradeHistoryItems, x => x.PaddleSubscriptions);
            currentPlanQuery = currentPlanQuery.Where(x => x.UserId == userId);

            return await AsyncExecuter.FirstOrDefaultAsync(currentPlanQuery);
        }

        public async Task<UserPlanEntity> GetCurrentPlanByPaddleSubId(long subscriptionId)
        {
            var query = from p in await _userPlanPaddleSubscriptionRepository.GetQueryableAsync()
                        join up in await _userPlanRepository.WithDetailsAsync(x => x.PaddleSubscriptions, x => x.UpgradeHistoryItems) on p.UserId equals up.UserId
                        where p.IsCurrent == true && p.SubscriptionId == subscriptionId
                        select up;

            return await AsyncExecuter.FirstOrDefaultAsync(query);
        }

        public Task<string> GetPaddleReferenceAsync(long? subscriptionPaymentId, long subscriptionId)
        {
            return Task.FromResult($"payment_method=paddle&subscription_payment_id={subscriptionPaymentId};subscription_id={subscriptionId}");
        }

        public Task<(string, string)> ParsePaddleReference(string reference)
        {
            string subscriptionPaymentId = string.Empty;
            string subscriptionId = string.Empty;

            if (reference.IsEmpty())
            {
                return Task.FromResult((subscriptionPaymentId, subscriptionId));
            }

            var pairs = reference.Split(';');
            foreach (var item in pairs)
            {
                var kv = item.Split('=');
                if (kv.Length != 2)
                {
                    continue;
                }

                var key = kv[0];
                var value = kv[1];

                switch (key)
                {
                    case "subscription_payment_id":
                        subscriptionPaymentId = value;
                        break;

                    case "subscription_id":
                        subscriptionId = value;
                        break;

                    default:
                        break;
                }
            }

            return Task.FromResult((subscriptionPaymentId, subscriptionId));
        }

        private bool IsExpired(DateTime expiredDate)
        {
            return Clock.Now > expiredDate;
        }

        public async Task AddCancelationSurvey(Guid userId, string reasonType, string reasonText, string feedback)
        {
            await _userPlanCancelationSurveyRepository.InsertAsync(new UserPlanCancelationSurveyEntity()
            {
                UserId = userId,
                ReasonType = reasonType,
                ReasonText = reasonText,
                Feedback = feedback
            });
        }
    }
}
