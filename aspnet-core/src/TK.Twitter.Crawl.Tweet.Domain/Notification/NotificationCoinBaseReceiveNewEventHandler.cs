using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet.Slack;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Notification
{
    public class NotificationCoinBaseReceiveNewEventHandler : IDistributedEventHandler<NotificationCoinBaseReceiveNewEventEto>, ITransientDependency
    {
        private readonly IRepository<CoinBaseWebhookLogEntity, Guid> _coinBaseWebhookLogRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly SlackService _slackService;

        public NotificationCoinBaseReceiveNewEventHandler(
            IRepository<CoinBaseWebhookLogEntity, Guid> coinBaseWebhookLogRepository,
            IRepository<IdentityUser, Guid> userRepository,
            UserManager<IdentityUser> userManager,
            IUnitOfWorkManager unitOfWorkManager,
            HttpClient httpClient,
            SlackService slackService
            )
        {
            _coinBaseWebhookLogRepository = coinBaseWebhookLogRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _unitOfWorkManager = unitOfWorkManager;
            HttpClient = httpClient;
            _slackService = slackService;
        }

        public HttpClient HttpClient { get; }

        public async Task HandleEventAsync(NotificationCoinBaseReceiveNewEventEto eventData)
        {
            using var uow = _unitOfWorkManager.Begin();
            var webhookLog = await _coinBaseWebhookLogRepository.FirstOrDefaultAsync(x => x.EventId == eventData.EventId && x.EventType == eventData.EventType);
            if (webhookLog == null)
            {
                return;
            }

            switch (webhookLog.EventType)
            {
                case "charge:confirmed":
                    await NotifyWhilePaymentSucceeded(webhookLog.Raw);
                    break;
                default:
                    break;
            }
        }

        private async Task NotifyWhilePaymentSucceeded(string raw)
        {
            try
            {
                var jObject = JObject.Parse(raw);

                var sb = new StringBuilder();

#if DEBUG
                sb.AppendLine($"(Testing mode)");
#endif


                var sb2 = new StringBuilder();
                var payments = jObject["event"]["data"]["payments"];

                foreach (var item in payments)
                {
                    var local = item["value"]["local"];
                    var crypto = item["value"]["crypto"];

                    var cryptoVal = $"{crypto["amount"].ParseIfNotNull<decimal>()} {crypto["currency"].ParseIfNotNull<string>()}";
                    var localVal = $"{local["amount"].ParseIfNotNull<decimal>()} {local["currency"].ParseIfNotNull<string>()}";
                    sb2.AppendLine($"● {cryptoVal} (~ {localVal})");
                }

                var email = jObject["event"]["data"]["metadata"]["email"].ParseIfNotNull<string>();
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return;
                }

                sb.AppendLine($"[Lead3] Payment Succeeded");
                sb.AppendLine($"Email: " + email);
                sb.AppendLine($"Event Time: " + jObject["event"]["created_at"].ParseIfNotNull<DateTime>().ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine("Payments:");
                sb.AppendLine(sb2.ToString());

                var message = sb.ToString();
                await _slackService.SendAsync(message);
            }
            catch (Exception ex)
            {

            }
        }

    }
}
