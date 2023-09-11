using Microsoft.EntityFrameworkCore;
using TK.Telegram.Domain;
using TK.Telegram.Domain.Entities;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace TK.Telegram.EntityFrameworkCore
{
    [ConnectionStringName(TelegramDbProperties.ConnectionStringName)]
    public class TelegramDbContext : AbpDbContext<TelegramDbContext>, ITelegramDbContext
    {
        public TelegramDbContext(DbContextOptions<TelegramDbContext> options)
            : base(options)
        {

        }

        public DbSet<TelegramBotSendingQueueEntity> TelegramBotSendQueueEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigureTelegramManagement();
        }
    }


}