using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TK.Telegram.Domain.Entities;
using TK.Telegram.Domain.Service;
using TK.Twitter.Crawl.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Notification
{
    [EventName("Notification.ErrorEto")]
    public class NotificationErrorEto
    {
        public string Tags { get; set; }

        public string Message { get; set; }

        public string ExceptionStackTrace { get; set; }
    }

    public class NotificationErrorHandler : IDistributedEventHandler<NotificationErrorEto>, ITransientDependency
    {
        private static Regex TagRegex = new Regex(@"(\@[a-zA-Z_]+\b)(?!;)");
        private static Random Random = new Random();

        private readonly IOptions<NotificationOptions> _options;
        private readonly ITelegramBotSender _telegramBotSender;
        private readonly IRepository<TelegramBotSendingQueueEntity, long> _telegramBotSendingQueueRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public NotificationErrorHandler(
            IClock clock,
            IOptions<NotificationOptions> options,
            ITelegramBotSender telegramBotSender,
            ILogger<NotificationErrorHandler> logger,
            IRepository<TelegramBotSendingQueueEntity, long> telegramBotSendingQueueRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            Clock = clock;
            _options = options;
            _telegramBotSender = telegramBotSender;
            Logger = logger;
            _telegramBotSendingQueueRepository = telegramBotSendingQueueRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public IClock Clock { get; }
        public ILogger<NotificationErrorHandler> Logger { get; }

        public async Task HandleEventAsync(NotificationErrorEto eventData)
        {
            var telegramPrivateChatId = _options.Value.TelegramChatId;

            using var uow = _unitOfWorkManager.Begin();
            try
            {
                string msg = GetMessageTemplate(eventData);

#if DEBUG
                try
                {
                    await _telegramBotSender.SendAsync(telegramPrivateChatId, msg, ParseMode.Html);
                }
                catch (Exception)
                {

                }
#else
                // random giây đc thêm
                //var addingSecs = Random.Next(180, 300);
                //lastTime = lastTime.AddSeconds(addingSecs);
                await _telegramBotSender.QueueAsync(telegramPrivateChatId, msg, ParseMode.Html);
#endif

                await uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while prepare add queue message");
                await uow.RollbackAsync();
            }
        }

        public static string GetMessageTemplate(NotificationErrorEto error)
        {
            var sb = new StringBuilder();
#if DEBUG
            sb.AppendLine($"<b>(Testing mode)</b>");
#endif

            sb.AppendLine($"⚑<b>{error.Tags}</b>");
            sb.AppendLine($"⚠{error.Message}");
            if (!error.ExceptionStackTrace.IsNullOrWhiteSpace())
            {
                sb.AppendLine($@"👣<pre> {error.ExceptionStackTrace}</pre>");
            }

            return sb.ToString();
        }
    }
}
