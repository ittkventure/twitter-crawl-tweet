using Microsoft.EntityFrameworkCore;
using TK.Paddle.Domain;
using TK.Paddle.Domain.Entity;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace TK.Paddle.EntityFrameworkCore
{
    [ConnectionStringName(PaddleDbProperties.ConnectionStringName)]
    public interface IPaddleDbContext : IEfCoreDbContext
    {
        public DbSet<PaddleWebhookLogEntity> PaddleWebhookLogEntities { get; set; }
        public DbSet<PaddleWebhookProcessEntity> PaddleWebhookProcessEntities { get; set; }
    }
}
