using System.Runtime.CompilerServices;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TK.Telegram.Domain.Service
{
    public interface ITelegramBotSender
    {
        Task QueueAsync(string chatId, string text, ParseMode parseMode, DateTime? sentAfter = null);
        Task<Message> SendAsync(string chatId, string text, ParseMode parseMode, bool disableWebPagePreview = true);
    }
}