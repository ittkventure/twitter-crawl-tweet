using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TK.Telegram.Domain.Service;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Tweet.AirTable
{
    [EventName("AirTable.Webhook.AirTableAfterWebhookLogAddedEto")]
    public class AirTableAfterWebhookLogAddedEto
    {
        public Guid EventId { get; set; }

        public string SystemId { get; set; }

        public string Action { get; set; }
    }

    public class AirTableAfterWebhookLogAddedHandler : IDistributedEventHandler<AirTableAfterWebhookLogAddedEto>, ITransientDependency
    {
        private const string LOG_PREFIX = "[AirTableAfterWebhookLogAddedHandler] ";

        private readonly IRepository<AirTableWebhookLogEntity, Guid> _airTableWebhookLogRepository;
        private readonly IRepository<AirTableWebhookProcessEntity, long> _paymentWebhookProcessRepository;
        private readonly IRepository<TwitterUserSignalEntity, long> _twitterUserSignalRepository;
        private readonly IRepository<LeadEntity, long> _leadRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ITelegramBotSender _telegramBotSender;
        private readonly IOptions<NotificationOptions> _notifycationOptions;

        public AirTableAfterWebhookLogAddedHandler(
            IRepository<AirTableWebhookLogEntity, Guid> airTableWebhookLogRepository,
            IRepository<AirTableWebhookProcessEntity, long> paymentWebhookProcessRepository,
            IRepository<TwitterUserSignalEntity, long> twitterUserSignalRepository,
            IRepository<LeadEntity, long> leadRepository,
            ILogger<AirTableAfterWebhookLogAddedHandler> logger,
            IUnitOfWorkManager unitOfWorkManager,
            IClock clock,
            ITelegramBotSender telegramBotSender,
            IOptions<NotificationOptions> notifycationOptions)
        {
            _airTableWebhookLogRepository = airTableWebhookLogRepository;
            _paymentWebhookProcessRepository = paymentWebhookProcessRepository;
            _twitterUserSignalRepository = twitterUserSignalRepository;
            _leadRepository = leadRepository;
            _unitOfWorkManager = unitOfWorkManager;
            Logger = logger;
            Clock = clock;
            _telegramBotSender = telegramBotSender;
            _notifycationOptions = notifycationOptions;
        }

        public ILogger<AirTableAfterWebhookLogAddedHandler> Logger { get; }
        public IClock Clock { get; }

        public async Task HandleEventAsync(AirTableAfterWebhookLogAddedEto eventData)
        {
            using var uow = _unitOfWorkManager.Begin();
            try
            {
                var webhookLog = await _airTableWebhookLogRepository.FirstOrDefaultAsync(x => x.EventId == eventData.EventId);
                if (webhookLog == null)
                {
                    return;
                }

                var process = await _paymentWebhookProcessRepository.FirstOrDefaultAsync(x => x.EventId == eventData.EventId);
                if (process.Ended)
                {
                    return;
                }

                switch (webhookLog.Action)
                {
                    case "Created":
                    case "Updated":
                        await ProcessingAlert(process);
                        break;
                    default:
                        process.Note = "Skip by Action type";
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

        private async Task ProcessingAlert(AirTableWebhookProcessEntity processItem)
        {
            var uow = _unitOfWorkManager.Current;
            try
            {
                var lead = await _leadRepository.FirstOrDefaultAsync(x => x.UserId == processItem.SystemId);
                if (lead != null)
                {
                    var lastestSignalQuery = from sig in await _twitterUserSignalRepository.GetQueryableAsync()
                                             where sig.UserId == processItem.SystemId
                                             orderby sig.CreationTime descending
                                             select sig;

                    var signal = await _twitterUserSignalRepository.AsyncExecuter.FirstOrDefaultAsync(lastestSignalQuery);
                    var dto = new TelegramMessageDto()
                    {
                        ProjectUrl = lead.UserProfileUrl,
                        LastestSignal = CrawlConsts.Signal.GetName(signal.Signal),
                        LastestSignalFrom = lead.MediaMentioned,
                        SignalTime = lead.LastestSponsoredDate,
                        SignalUrl = lead.LastestSponsoredTweetUrl
                    };

                    string msg = GetMessageTemplate(dto);
#if DEBUG
                    try
                    {
                        await _telegramBotSender.SendAsync(_notifycationOptions.Value.Lead3ioChannelId, msg, ParseMode.Html);
                    }
                    catch (Exception)
                    {

                    }
#else
                        await _telegramBotSender.QueueAsync(_notifycationOptions.Value.Lead3ioChannelId, msg, ParseMode.Html);
#endif
                }

                processItem.Succeeded = true;
            }
            catch (BusinessException ex)
            {
                Logger.LogError(ex, LOG_PREFIX + $"An error occurred while processing {processItem.Action} {processItem.SystemId} with alert_id: {processItem.EventId}");
                processItem.Note = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LOG_PREFIX + $"An error occurred while processing {processItem.Action} {processItem.SystemId} with alert_id: {processItem.EventId}");
                processItem.Note = ex.Message;
            }

            // Tạm thời chỉ cho thực hiện 1 lần
            processItem.ProcessAttempt += 1;
            processItem.Ended = true;
            await _paymentWebhookProcessRepository.UpdateAsync(processItem);
        }

        private static string GetMessageTemplate(TelegramMessageDto input)
        {
            var sb = new StringBuilder();
#if DEBUG
            sb.AppendLine($"<b>(Testing mode)</b>");
#endif

            //🔥 2023-10-27 08:18 https://twitter.com/onigikitty Buying sponsored ads @ BSCNews
            //👉 Signal URL: https://twitter.com/BSCNews/status/1717639806304358723

            //:fire: {Date} {Twitter URL} {Signals} @ {Latest Signal from}
            //:point_right:Signal URL: {Signal URL}

            sb.AppendLine($"🔥 {input.SignalTime.Value.ToString("yyyy-MM-dd HH:mm")} <a href=\"{input.ProjectUrl}\">{input.ProjectUrl}</a> {input.LastestSignal} @ {input.LastestSignalFrom}");
            sb.AppendLine($"👉 Signal URL:  <a href=\"{input.SignalUrl}\">{input.SignalUrl}</a>");

            return sb.ToString();
        }

        private class TelegramMessageDto
        {
            public DateTime? SignalTime { get; set; }

            public string ProjectUrl { get; set; }

            public string LastestSignal { get; set; }

            public string LastestSignalFrom { get; set; }

            public string SignalUrl { get; set; }
        }
    }
}
