using Microsoft.EntityFrameworkCore;
using TK.Telegram.Domain.Entities;

namespace TK.Telegram.EntityFrameworkCore
{
    public interface ITelegramDbContext
    {
        DbSet<TelegramBotSendingQueueEntity> TelegramBotSendQueueEntities { get; set; }
    }
}