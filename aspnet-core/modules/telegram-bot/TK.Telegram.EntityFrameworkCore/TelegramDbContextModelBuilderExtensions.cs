using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TK.Telegram.Domain.Entities;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace TK.Telegram.EntityFrameworkCore
{
    public static class TelegramDbContextModelBuilderExtensions
    {
        public static void ConfigureTelegramManagement([NotNull] this ModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.Entity<TelegramBotSendingQueueEntity>(b =>
            {
                b.ToTable("telegram_bot_sending_queue");
                b.ConfigureByConvention();

                b.Property(x => x.ChatId).HasMaxLength(64);

                b.HasIndex(x => new { x.Succeeded, x.Ended });
            });

        }
    }


}