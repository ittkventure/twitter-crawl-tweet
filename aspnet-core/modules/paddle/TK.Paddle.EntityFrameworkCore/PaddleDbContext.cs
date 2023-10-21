using Microsoft.EntityFrameworkCore;
using TK.Paddle.Domain;
using TK.Paddle.Domain.Entity;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace TK.Paddle.EntityFrameworkCore
{
    [ConnectionStringName(PaddleDbProperties.ConnectionStringName)]
    public class PaddleDbContext : AbpDbContext<PaddleDbContext>, IPaddleDbContext
    {
        public PaddleDbContext(DbContextOptions<PaddleDbContext> options)
            : base(options)
        {

        }

        public DbSet<PaddleWebhookLogEntity> PaddleWebhookLogEntities { get; set; }
        public DbSet<PaddleWebhookProcessEntity> PaddleWebhookProcessEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigurePaddleManagement();
        }
    }
}
