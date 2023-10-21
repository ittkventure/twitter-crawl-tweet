using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TK.Paddle.Domain.Entity;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace TK.Paddle.EntityFrameworkCore
{
    public static class PaddleDbContextModelBuilderExtensions
    {
        public static void ConfigurePaddleManagement([NotNull] this ModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.Entity<PaddleWebhookLogEntity>(b =>
            {
                b.ToTable("paddle_webhook_log");
                b.ConfigureByConvention();

                b.HasIndex(x => new { x.AlertId, x.AlertName });
            });

            builder.Entity<PaddleWebhookProcessEntity>(b =>
            {
                b.ToTable("paddle_webhook_process");
                b.ConfigureByConvention();

                b.Property(x => x.Note).HasMaxLength(1000);

                b.HasIndex(x => new { x.AlertId, x.AlertName, x.Ended, x.Succeeded });
            });
        }
    }
}