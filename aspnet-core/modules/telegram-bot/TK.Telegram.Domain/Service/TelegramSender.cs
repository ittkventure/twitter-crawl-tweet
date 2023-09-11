using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TK.Telegram.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace TK.Telegram.Domain.Service
{
    public class TelegramBotSender : DomainService, ITelegramBotSender
    {
        private readonly IRepository<TelegramBotSendingQueueEntity, long> _telegramBotSendingQueueRepository;
        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramBotSender(
            IRepository<TelegramBotSendingQueueEntity, long> telegramBotSendingQueueRepository,
            ITelegramBotClient telegramBotClient)
        {
            _telegramBotSendingQueueRepository = telegramBotSendingQueueRepository;
            _telegramBotClient = telegramBotClient;
        }

        public async Task QueueAsync(string chatId, string text, ParseMode parseMode, DateTime? sentAfter = null)
        {
            var queueItem = new TelegramBotSendingQueueEntity()
            {
                ChatId = chatId,
                TextContent = text,
                ParseMode = (int)parseMode,
            };

            await _telegramBotSendingQueueRepository.InsertAsync(queueItem);
        }

        public async Task<Message> SendAsync(string chatId, string text, ParseMode parseMode, bool disableWebPagePreview = true)
        {
            return await _telegramBotClient.SendTextMessageAsync(new ChatId(chatId), text, parseMode: parseMode, disableWebPagePreview: disableWebPagePreview);
        }

    }
}