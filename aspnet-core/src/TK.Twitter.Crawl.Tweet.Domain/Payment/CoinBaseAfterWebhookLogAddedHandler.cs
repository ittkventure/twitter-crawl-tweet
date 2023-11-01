using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Notification;
using TK.Twitter.Crawl.Tweet.User;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Tweet.Payment
{
    public class CoinBaseAfterWebhookLogAddedHandler : IDistributedEventHandler<CoinBaseAfterWebhookLogAddedEto>, ITransientDependency
    {
        private const string LOG_PREFIX = "[CoinBaseAfterWebhookLogAddedHandler] ";

        private readonly UserPlanManager _userPlanManager;
        private readonly IRepository<CoinBaseWebhookLogEntity, Guid> _coinBaseWebhookLogRepository;
        private readonly IRepository<CoinBaseWebhookProcessEntity, long> _paymentWebhookProcessRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly IRepository<UserPlanEntity, Guid> _userPlanRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IDistributedEventBus _distributedEventBus;

        public CoinBaseAfterWebhookLogAddedHandler(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            IOptions<IdentityOptions> identityOptions,
            IDistributedEventBus distributedEventBus,
            UserPlanManager userPlanManager,
            IRepository<CoinBaseWebhookLogEntity, Guid> coinBaseWebhookLogRepository,
            IRepository<CoinBaseWebhookProcessEntity, long> paymentWebhookProcessRepository,
            IRepository<IdentityUser, Guid> userRepository,
            IRepository<UserPlanEntity, Guid> userPlanRepository,
            ILogger<CoinBaseAfterWebhookLogAddedHandler> logger,
            IUnitOfWorkManager unitOfWorkManager,
            IGuidGenerator guidGenerator,
            IConfiguration configuration,
            IClock clock)
        {
            _userPlanManager = userPlanManager;
            _coinBaseWebhookLogRepository = coinBaseWebhookLogRepository;
            _paymentWebhookProcessRepository = paymentWebhookProcessRepository;
            _userRepository = userRepository;
            _userPlanRepository = userPlanRepository;
            Logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
            _guidGenerator = guidGenerator;
            _configuration = configuration;
            _userManager = userManager;
            _emailSender = emailSender;
            _identityOptions = identityOptions;
            _distributedEventBus = distributedEventBus;
            Clock = clock;
        }

        public ILogger<CoinBaseAfterWebhookLogAddedHandler> Logger { get; }
        public IClock Clock { get; }

        public async Task HandleEventAsync(CoinBaseAfterWebhookLogAddedEto eventData)
        {
            using var uow = _unitOfWorkManager.Begin();
            try
            {
                var webhookLog = await _coinBaseWebhookLogRepository.FirstOrDefaultAsync(x => x.EventId == eventData.EventId && x.EventType == eventData.EventType);
                if (webhookLog == null)
                {
                    return;
                }

                var process = await _paymentWebhookProcessRepository.FirstOrDefaultAsync(x => x.EventId == eventData.EventId
                                                                                           && x.EventType == eventData.EventType);
                if (process.Ended)
                {
                    return;
                }

                switch (webhookLog.EventType)
                {
                    case "charge:confirmed":
                        await ProcessSubsciptionPaymentSucceeded(webhookLog.Raw);

                        await _distributedEventBus.PublishAsync(new NotificationCoinBaseReceiveNewEventEto()
                        {
                            EventId = eventData.EventId,
                            EventType = eventData.EventType,
                        });

                        break;
                    default:
                        process.Note = "Skip by event type";
                        process.Ended = true;
                        process.Succeeded = true;

                        await _paymentWebhookProcessRepository.UpdateAsync(process);
                        break;
                }

                await uow.SaveChangesAsync();
                await uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LOG_PREFIX + "An error while Process Webhook event");
                await uow.RollbackAsync();
            }
        }

        private async Task ProcessSubsciptionPaymentSucceeded(string raw)
        {
            var uow = _unitOfWorkManager.Current;
            var jObject = JObject.Parse(raw);

            var eventId = jObject["id"].ParseIfNotNull<string>();
            string eventType = jObject["event"]?["type"].ParseIfNotNull<string>();

            var data = jObject["event"]["data"];

            CoinBaseWebhookProcessEntity processItem = null;
            try
            {
                processItem = await _paymentWebhookProcessRepository.FirstOrDefaultAsync(x => x.EventId == eventId && x.EventType == eventType);

                var planId = data["checkout"]["id"].ParseIfNotNull<string>();

                string planKey = CrawlConsts.CoinBase.GetPlanKey(planId, _configuration);

                if (!CrawlConsts.Payment.PAID_PLAN.Contains(planKey))
                {
                    throw new BusinessException(CrawlDomainErrorCodes.PaymentInvalidPlan, "Invalid Plan");
                }

                bool isStdPlan = CrawlConsts.CoinBase.IsStandardPlan(planId, _configuration);

                decimal planPrice = CrawlConsts.CoinBase.GetPlanPrice(planKey, _configuration);

                var email = data["metadata"]["email"].ParseIfNotNull<string>();
                var user = await RegisterWithoutPasswordAsync(email, isStdPlan);

                var currentPlan = await _userPlanManager.UpgradeOrRenewalPlan(user.Id, planKey, PaymentMethod.CoinBase, historyRef: "from-coin-base=true");
                currentPlan.PaymentMethod = (int)PaymentMethod.CoinBase;
                await _userPlanRepository.UpdateAsync(currentPlan);

                processItem.Succeeded = true;
            }
            catch (BusinessException ex)
            {
                Logger.LogError(ex, LOG_PREFIX + $"An error occurred while processing {eventType} with alert_id: {eventId}");
                processItem.Note = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LOG_PREFIX + $"An error occurred while processing {eventType} with alert_id: {eventId}");
                processItem.Note = ex.Message;
            }

            // Tạm thời chỉ cho thực hiện 1 lần
            processItem.ProcessAttempt += 1;
            processItem.Ended = true;
            await _paymentWebhookProcessRepository.UpdateAsync(processItem);
        }

        public async Task<IdentityUser> RegisterWithoutPasswordAsync(string email, bool stdPlan, bool autoSave = true)
        {
            var emailAlreadyExist = await _userRepository.FirstOrDefaultAsync(x => x.NormalizedEmail == email.ToUpper());
            if (emailAlreadyExist != null)
            {
                return emailAlreadyExist;
            }

            var user = new IdentityUser(id: _guidGenerator.Create(), userName: email, email: email);

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                throw new BusinessException(CrawlDomainErrorCodes.InsideLogicError, result.Errors.JoinAsString(Environment.NewLine));
            }

            // Gửi email thông báo đăng ký thành công kèm link để đổi mật khẩu
            user = await _userManager.FindByEmailAsync(email);

            // Tạo reset password để khi user redirect lại trang sẽ đến trang đổi mật khẩu luôn
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await SendEmailWelCome(email, stdPlan, token);

            if (autoSave)
            {
                var uow = _unitOfWorkManager.Current;
                await uow.SaveChangesAsync();
            }

            return user;
        }

        public async Task SendEmailWelCome(string email, bool stdPlan, string resetPwdToken)
        {
            // nếu là plan trial thì vẫn nó tương đương với premium
            string subject = "[Lead3] You're Now a Standard Member";
            if (!stdPlan)
            {
                subject = "[Lead3] You're Now a Premium Member";
            }

            string setNewPasswordUrl = $"https://lead3.io/user/change-password?email={email}&token=" + resetPwdToken;

            string html = $@"<p data-pm-slice=""""><span style=""font-size: 12pt;"">Hey,</span></p>
<p><span style=""font-size: 12pt;"">Awesome news - you are now a {(stdPlan ? "Standard" : "Premium")} Member of Lead3! We&rsquo;re thrilled to have you on board.</span></p>
<p><span style=""font-size: 12pt;"">Here&rsquo;s what you need to do next:</span></p>
<ol class="""">
<li class="""" data-list-indent="""" data-list-type="""">
<p><span style=""font-size: 12pt;""><strong>Set new password for your account</strong>: Follow this link to set password for your lead3 account {email} </span><br /><span style=""font-size: 12pt;"">Link: <br /> <a target=""_blank"" href=""{setNewPasswordUrl}"" rel="""">{setNewPasswordUrl}</a></span></p>
</li>
<li class="""" data-list-indent="""" data-list-type="""">
<p><span style=""font-size: 12pt;""><strong>Login to your account:</strong> After setting your password, login to your account using your new credentials</span></p>
</li>
<li class="""" data-list-indent="""" data-list-type="""">
<p><span style=""font-size: 12pt;""><strong>Access the Lead Database:</strong> You'll find the leads database in your user page. Feel free to click on ""view larger version"" if you want to use that data in Airtable.</span></p>
</li>
</ol>
<p><span style=""font-size: 12pt;"">If you need any help using the list, have any questions, or wish to provide feedback and suggestions, please don&rsquo;t hesitate to reach out to our friendly Customer Service team.</span></p>
<p><span style=""font-size: 12pt;""><strong>We&rsquo;ll send the product updates from this email address:</strong></span></p>
<ul>
<li><span style=""font-size: 12pt;""><a href=""mailto:contact@lead3.io"">contact@lead3.io</a></span></li>
</ul>
<p><span style=""font-size: 12pt;"">To ensure our emails don't end up in your Spam folder, please reply with ""OK"" to this email. It helps ensure you receive our updates. If this email ends up in your promotions or spam folder, kindly move it to your Primary inbox.</span></p>
<p><span style=""font-size: 12pt;"">Thank you for choosing Lead3. We're excited to be part of your journey in expanding your client and partner base.</span></p>
<p><span style=""font-size: 12pt;"">Your success is our success, and we encourage you to share your success stories with us if you win any deals from our leads.</span></p>
<p><span style=""font-size: 12pt;"">Best,</span></p>
<p><span style=""font-size: 12pt;"">The Lead3 team</span></p>";

            await _emailSender.SendAsync(email, subject, html);
        }
    }
}
