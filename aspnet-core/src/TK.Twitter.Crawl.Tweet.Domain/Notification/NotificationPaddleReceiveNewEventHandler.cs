using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TK.Paddle.Domain.Entity;
using TK.Paddle.Domain.Shared;
using TK.Paddle.Domain;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using TK.Twitter.Crawl.Tweet.Slack;
using Microsoft.AspNetCore.Identity;

namespace TK.Twitter.Crawl.Notification
{
    public class NotificationPaddleReceiveNewEventHandler : IDistributedEventHandler<NotificationPaddleReceiveNewEventEto>, ITransientDependency
    {
        private readonly IRepository<PaddleWebhookLogEntity, Guid> _paddleWebhookLogRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PaddlePaymentManager _paddlePaymentManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly SlackService _slackService;

        public NotificationPaddleReceiveNewEventHandler(
            IRepository<PaddleWebhookLogEntity, Guid> paddleWebhookLogRepository,
            IRepository<IdentityUser, Guid> userRepository,
            UserManager<IdentityUser> userManager,
            PaddlePaymentManager paddlePaymentManager,
            IUnitOfWorkManager unitOfWorkManager,
            HttpClient httpClient,
            SlackService slackService
            )
        {
            _paddleWebhookLogRepository = paddleWebhookLogRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _paddlePaymentManager = paddlePaymentManager;
            _unitOfWorkManager = unitOfWorkManager;
            HttpClient = httpClient;
            _slackService = slackService;
        }

        public HttpClient HttpClient { get; }

        public async Task HandleEventAsync(NotificationPaddleReceiveNewEventEto eventData)
        {
            using var uow = _unitOfWorkManager.Begin();
            var webhookLog = await _paddleWebhookLogRepository.FirstOrDefaultAsync(x => x.AlertId == eventData.AlertId && x.AlertName == eventData.AlertName);
            if (webhookLog == null)
            {
                return;
            }

            switch (webhookLog.AlertName)
            {
                case PaddleWebhookConst.AlertName.SUBSCRIPTION_PAYMENT_SUCCEEDED:
                    await NotifyWhilePaymentSucceeded(webhookLog.Raw);
                    break;
                case PaddleWebhookConst.AlertName.SUBSCRIPTION_CANCELLED:
                    await NotifyWhileSubscriptionCanceled(webhookLog.Raw);
                    break;
                default:
                    break;
            }
        }

        private async Task NotifyWhilePaymentSucceeded(string raw)
        {
            var input = PaddleDataAdapter.GetSubscriptionPaymentSuccessInput(raw);

            var sb = new StringBuilder();

#if DEBUG
            sb.AppendLine($"(Testing mode)");
#endif

            //var (orderId, userId) = await _paddlePaymentManager.GetPassthroughDataAsync(input.Passthrough);
            //if (userId == Guid.Empty)
            //{
            //    throw new BusinessException(CrawlDomainErrorCodes.PaymentCanNotParsePassthroughtData, "Passthrough data invalid");
            //}

            var user = await _userManager.FindByEmailAsync(input.Email);
            if (user == null)
            {
                return;
            }

            sb.AppendLine($"Payment Succeeded");
            sb.AppendLine($"Username: " + user.UserName);
            sb.AppendLine($"Email: " + input.Email);
            sb.AppendLine($"Earn: {input.Earnings}$");
            sb.AppendLine($"Event Time: " + input.EventTime?.ToString("yyyy-MM-dd HH:mm:ss"));

            var message = sb.ToString();
            await _slackService.SendAsync(message);
        }

        private async Task NotifyWhileSubscriptionCanceled(string raw)
        {
            var input = PaddleDataAdapter.GetSubscriptionCanceledInput(raw);
            var sb = new StringBuilder();

#if DEBUG
            sb.AppendLine($"(Testing mode)");
#endif

            //var (orderId, userId) = await _paddlePaymentManager.GetPassthroughDataAsync(input.Passthrough);
            //if (userId == Guid.Empty)
            //{
            //    throw new BusinessException(CrawlDomainErrorCodes.PaymentCanNotParsePassthroughtData, "Passthrough data invalid");
            //}

            //var user = await _userRepository.GetAsync(userId.Value);


            var user = await _userManager.FindByEmailAsync(input.Email);
            if (user == null)
            {
                return;
            }

            sb.AppendLine($"Subscription Canceled");
            sb.AppendLine($"Username: " + user.UserName);
            sb.AppendLine($"Email: " + input.Email);
            sb.AppendLine($"Event Time: " + input.EventTime?.ToString("yyyy-MM-dd HH:mm:ss"));

            var message = sb.ToString();
            await _slackService.SendAsync(message);
        }
    }
}
