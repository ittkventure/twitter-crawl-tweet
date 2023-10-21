using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.Paddle.Domain.Entity;
using TK.Paddle.Domain.Eto;
using TK.Paddle.Domain.Shared;
using TK.Paddle.Domain;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet.User;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp;
using TK.Twitter.Crawl.Notification;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Volo.Abp.Emailing;
using Volo.Abp.Guids;
using Microsoft.Extensions.Configuration;

namespace TK.Twitter.Crawl.Tweet.Payment
{
    public class PaddleAfterWebhookLogAddedHandler : IDistributedEventHandler<PaddleAfterWebhookLogAddedEto>, ITransientDependency
    {
        private const string LOG_PREFIX = "[PaddleAfterWebhookLogAddedHandler] ";

        private readonly UserPlanManager _userPlanManager;
        private readonly IRepository<PaddleWebhookLogEntity, Guid> _paddleWebhookLogRepository;
        private readonly IRepository<PaddleWebhookProcessEntity, long> _paymentWebhookProcessRepository;
        private readonly IRepository<PaymentOrderEntity, Guid> _paymentOrderRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly PaddlePaymentManager _paddlePaymentManager;
        private readonly IRepository<UserPlanEntity, Guid> _userPlanRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IDistributedEventBus _distributedEventBus;

        public PaddleAfterWebhookLogAddedHandler(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            IOptions<IdentityOptions> identityOptions,
            IDistributedEventBus distributedEventBus,
            UserPlanManager userPlanManager,
            IRepository<PaddleWebhookLogEntity, Guid> paddleWebhookLogRepository,
            IRepository<PaddleWebhookProcessEntity, long> paymentWebhookProcessRepository,
            IRepository<PaymentOrderEntity, Guid> paymentOrderRepository,
            IRepository<IdentityUser, Guid> userRepository,
            PaddlePaymentManager paddlePaymentManager,
            IRepository<UserPlanEntity, Guid> userPlanRepository,
            ILogger<PaddleAfterWebhookLogAddedHandler> logger,
            IUnitOfWorkManager unitOfWorkManager,
            IGuidGenerator guidGenerator,
            IConfiguration configuration,
            IClock clock)
        {
            _userPlanManager = userPlanManager;
            _paddleWebhookLogRepository = paddleWebhookLogRepository;
            _paymentWebhookProcessRepository = paymentWebhookProcessRepository;
            _paymentOrderRepository = paymentOrderRepository;
            _userRepository = userRepository;
            _paddlePaymentManager = paddlePaymentManager;
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

        public ILogger<PaddleAfterWebhookLogAddedHandler> Logger { get; }
        public IClock Clock { get; }

        public async Task HandleEventAsync(PaddleAfterWebhookLogAddedEto eventData)
        {
            using var uow = _unitOfWorkManager.Begin();
            try
            {
                var webhookLog = await _paddleWebhookLogRepository.FirstOrDefaultAsync(x => x.AlertId == eventData.AlertId && x.AlertName == eventData.AlertName);
                if (webhookLog == null)
                {
                    return;
                }

                var processed = await _paymentWebhookProcessRepository.AnyAsync(
                    x => x.AlertId == eventData.AlertId && x.AlertName == eventData.AlertName && x.Ended == true
                    );

                if (processed)
                {
                    return;
                }

                switch (webhookLog.AlertName)
                {
                    case PaddleWebhookConst.AlertName.SUBSCRIPTION_PAYMENT_SUCCEEDED:
                        await ProcessSubsciptionPaymentSucceeded(webhookLog.Raw);
                        break;
                    case PaddleWebhookConst.AlertName.SUBSCRIPTION_CANCELLED:
                        await ProcessSubsciptionCanceled(webhookLog.Raw);
                        break;
                    default:
                        break;
                }

                await _distributedEventBus.PublishAsync(new NotificationPaddleReceiveNewEventEto()
                {
                    AlertId = eventData.AlertId,
                    AlertName = eventData.AlertName,
                });

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
            var input = PaddleDataAdapter.GetSubscriptionPaymentSuccessInput(raw);

            PaddleWebhookProcessEntity processItem = null;
            try
            {
                processItem = await _paymentWebhookProcessRepository.FirstOrDefaultAsync(x => x.AlertId == input.AlertId && x.AlertName == input.AlertName);

                string planKey = CrawlConsts.Payment.GetPlanKey(input.SubscriptionPlanId, _configuration);

                if (!CrawlConsts.Payment.PAID_PLAN.Contains(planKey))
                {
                    throw new BusinessException(CrawlDomainErrorCodes.PaymentInvalidPlan, "Invalid Plan");
                }

                var (orderId, userId) = await _paddlePaymentManager.GetPassthroughDataAsync(input.Passthrough);
                if (orderId == Guid.Empty)
                {
                    throw new BusinessException(CrawlDomainErrorCodes.PaymentCanNotParsePassthroughtData, "Passthrough data invalid");
                }

                var order = await _paymentOrderRepository.FirstOrDefaultAsync(x => x.OrderId == orderId);
                if (order == null)
                {
                    throw new BusinessException(CrawlDomainErrorCodes.PaymentOrderNotFound, "Order not found");
                }

                if (order.Email != input.Email)
                {
                    throw new BusinessException(CrawlDomainErrorCodes.PaymentOrderNotBelongToUser, "Order not belong to user");
                }

                if (order.PaymentMethod != (int)PaymentMethod.Paddle)
                {
                    throw new BusinessException(CrawlDomainErrorCodes.PaymentPaymentMethodInvalid, "Payment method invalid");
                }

                bool isStdPlan = CrawlConsts.Payment.IsStdPlan(input.SubscriptionPlanId, _configuration);

                decimal planPrice = CrawlConsts.Payment.GetPlanPrice(planKey, _configuration);

                var user = await RegisterWithoutPasswordAsync(input.Email, isStdPlan);

                string refValue = await _userPlanManager.GetPaddleReferenceAsync(input.SubscriptionPaymentId, input.SubscriptionId);

                if (input.Status == PaddleConst.SubscriptionStatus.ACTIVE)
                {
                    if (input.SaleGross >= planPrice)
                    {
                        var currentPlan = await _userPlanManager.UpgradeOrRenewalPlan(user.Id, planKey, historyRef: refValue);
                        currentPlan.PaddleAddSubscription(input.SubscriptionId);
                        currentPlan.PaymentMethod = (int)PaymentMethod.Paddle;
                        await _userPlanRepository.UpdateAsync(currentPlan);

                        order.OrderStatusId = (int)PaymentOrderStatus.Completed;
                    }
                    else if (input.SaleGross > 0)
                    {
                        order.OrderStatusId = (int)PaymentOrderStatus.PatialPayment;
                    }
                    else
                    {
                        order.OrderStatusId = (int)PaymentOrderStatus.WaitingPayment;
                    }
                    await _paymentOrderRepository.UpdateAsync(order);
                }

                order.AddPaddlePaymentInfo(new PaymentOrderPaddleEntity()
                {
                    BalanceCurrency = input.BalanceCurrency,
                    BalanceEarnings = input.BalanceEarnings,
                    BalanceFee = input.BalanceFee,
                    BalanceGross = input.BalanceGross,
                    BalanceTax = input.BalanceTax,
                    CheckoutId = input.CheckoutId,
                    Country = input.Country,
                    Coupon = input.Coupon,
                    Currency = input.Currency,
                    CustomData = input.CustomData,
                    CustomerName = input.CustomerName,
                    Earnings = input.Earnings,
                    Email = input.Email,
                    EventTime = input.EventTime,
                    Fee = input.Fee,
                    Ip = input.Ip,
                    MarketingConsent = input.MarketingConsent,
                    Passthrough = input.Passthrough,
                    PaddleOrderId = input.OrderId,
                    PaymentMethod = input.PaymentMethod,
                    PaymentTax = input.PaymentTax,
                    ProductId = input.ProductId,
                    ProductName = input.ProductName,
                    Quantity = input.Quantity,
                    ReceiptUrl = input.ReceiptUrl,
                    SaleGross = input.SaleGross,
                    UsedPriceOverride = input.UsedPriceOverride,
                    InitialPayment = input.InitialPayment,
                    Instalments = input.Instalments,
                    NextBillDate = input.NextBillDate,
                    NextPaymentAmount = input.NextPaymentAmount,
                    PlanName = input.PlanName,
                    Status = input.Status,
                    SubscriptionId = input.SubscriptionId,
                    SubscriptionPaymentId = input.SubscriptionPaymentId,
                    SubscriptionPlanId = input.SubscriptionPlanId,
                    UnitPrice = input.UnitPrice,
                    UserId = input.UserId,
                });

                processItem.Succeeded = true;
            }
            catch (BusinessException ex)
            {
                Logger.LogError(ex, LOG_PREFIX + $"An error occurred while processing {input.AlertName} with alert_id: {input.AlertId}");
                processItem.Note = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LOG_PREFIX + $"An error occurred while processing {input.AlertName} with alert_id: {input.AlertId}");
                processItem.Note = ex.Message;
            }

            // Tạm thời chỉ cho thực hiện 1 lần
            processItem.ProcessAttempt += 1;
            processItem.Ended = true;
            await _paymentWebhookProcessRepository.UpdateAsync(processItem);
        }

        private async Task ProcessSubsciptionCanceled(string raw)
        {
            var uow = _unitOfWorkManager.Current;
            var input = PaddleDataAdapter.GetSubscriptionCanceledInput(raw);

            PaddleWebhookProcessEntity processItem = null;
            try
            {
                processItem = await _paymentWebhookProcessRepository.FirstOrDefaultAsync(x => x.AlertId == input.AlertId && x.AlertName == input.AlertName);
                var (_, userId) = await _paddlePaymentManager.GetPassthroughDataAsync(input.Passthrough);

                var userPlan = await _userPlanManager.GetCurrentPlan(userId.Value);
                if (userPlan == null)
                {
                    throw new BusinessException(CrawlDomainErrorCodes.NotFound, "User plan not found. Subscription ID: " + input.SubscriptionId);
                }

                userPlan.PaddleCancelSubsciption(input.SubscriptionId, input.CancellationEffectiveDate);

                string historyRef = await _userPlanManager.GetPaddleReferenceAsync(subscriptionPaymentId: null, input.SubscriptionId);

                userPlan.AddHistory(new UserPlanUpgradeHistoryEntity()
                {
                    UserId = userPlan.UserId,
                    Type = CrawlConsts.Payment.Type.CANCEL,
                    OldPlanKey = userPlan.PlanKey,
                    NewPlanKey = CrawlConsts.Payment.FREE,
                    CreatedAt = Clock.Now,
                    Reference = historyRef
                });

                await _userPlanRepository.UpdateAsync(userPlan);

                processItem.Succeeded = true;
            }
            catch (BusinessException ex)
            {
                Logger.LogError(ex, LOG_PREFIX + $"An error occurred while processing {input.AlertName} with alert_id: {input.AlertId}");
                processItem.Note = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LOG_PREFIX + $"An error occurred while processing {input.AlertName} with alert_id: {input.AlertId}");
                processItem.Note = ex.Message;
            }

            // Tạm thời chỉ cho thực hiện 1 lần
            processItem.ProcessAttempt += 1;
            processItem.Ended = true;
            await _paymentWebhookProcessRepository.UpdateAsync(processItem);
        }

        public async Task<IdentityUser> RegisterWithoutPasswordAsync(string email, bool stdPlan)
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

            string subject = "[Lead3] You're Now a Standard Member";
            if (stdPlan)
            {
                subject = "[Lead3] You're Now a Premium Member";
            }

            string setNewPasswordUrl = $"https://lead3.io/user/change-password?email={email}&token=" + token;

            await _emailSender.SendAsync(email,
                subject,
                $@"<p data-pm-slice=""1 1 []"">Hey,</p>
<p>&nbsp;</p>
<p>Awesome news - you are now a {(stdPlan ? "Standard" : "Premium")} Member of Lead3! We&rsquo;re thrilled to have you on board.</p>
<p>&nbsp;</p>
<p>Here&rsquo;s what you need to do next:</p>
<ol class=""ProsemirrorEditor-list"">
<li class=""ProsemirrorEditor-listItem"" data-list-indent=""1"" data-list-type=""numbered"">
<p><strong>Set new password for your account</strong>: Follow this link to set password for your lead3 account {email} <br />Link: <a href=""{setNewPasswordUrl}"" target=""_blank"" rel=""noopener"">here</a></p>
</li>
<li class=""ProsemirrorEditor-listItem"" data-list-indent=""1"" data-list-type=""numbered"">
<p><strong>Login to your account:</strong> After setting your password, login to your account using your new credentials</p>
</li>
<li class=""ProsemirrorEditor-listItem"" data-list-indent=""1"" data-list-type=""numbered"">
<p><strong>Access the Lead Database:</strong> You'll find the leads database in your user page. Feel free to click on ""view larger version"" if you want to use that data in Airtable.</p>
</li>
</ol>
<p>&nbsp;</p>
<p>If you need any help using the list, have any questions, or wish to provide feedback and suggestions, please don&rsquo;t hesitate to reach out to our friendly Customer Service team.</p>
<p>&nbsp;</p>
<p><strong>We&rsquo;ll send the product updates from this email address:</strong></p>
<ul>
<li><a class=""ProsemirrorEditor-link"" href=""mailto:contact@lead3.io"">contact@lead3.io</a></li>
</ul>
<p>&nbsp;</p>
<p>To ensure our emails don't end up in your Spam folder, please reply with ""OK"" to this email. It helps ensure you receive our updates. If this email ends up in your promotions or spam folder, kindly move it to your Primary inbox.</p>
<p>&nbsp;</p>
<p>Thank you for choosing Lead3. We're excited to be part of your journey in expanding your client and partner base.</p>
<p>&nbsp;</p>
<p>Your success is our success, and we encourage you to share your success stories with us if you win any deals from our leads.</p>
<p>&nbsp;</p>
<p>Best,</p>
<p>The Lead3 team</p>");

            var uow = _unitOfWorkManager.Current;
            await uow.SaveChangesAsync();

            return user;
        }
    }
}
