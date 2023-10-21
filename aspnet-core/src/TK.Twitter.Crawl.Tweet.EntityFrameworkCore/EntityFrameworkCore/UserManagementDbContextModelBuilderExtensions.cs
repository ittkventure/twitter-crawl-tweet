using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TK.Twitter.Crawl.Entity;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace TK.Twitter.Crawl.EntityFrameworkCore;

public static class UserManagementDbContextModelBuilderExtensions
{
    public static void ConfigureUserManagment([NotNull] this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<UserPlanEntity>(b =>
        {
            b.ToTable("user_plan");
            b.ConfigureByConvention();

            b.Property(x => x.PlanKey).HasMaxLength(256);

            b.HasIndex(x => new { x.UserId });
            b.HasIndex(x => new { x.UserId, x.PlanKey });
        });

        builder.Entity<UserPlanUpgradeHistoryEntity>(b =>
        {
            b.ToTable("user_plan_upgrade_history");
            b.ConfigureByConvention();

            b.Property(x => x.Type).HasMaxLength(64);
            b.Property(x => x.TimeAddedType).HasMaxLength(64);
            b.Property(x => x.OldPlanKey).HasMaxLength(256);
            b.Property(x => x.NewPlanKey).HasMaxLength(256);
            b.Property(x => x.Reference).HasMaxLength(1000);

            b.HasIndex(x => new { x.UserId });
            b.HasIndex(x => new { x.UserId, x.Type });
        });

        builder.Entity<UserPlanPaddleSubscriptionEntity>(b =>
        {
            b.ToTable("user_plan_paddle_subscription");
            b.ConfigureByConvention();

            b.HasIndex(x => new { x.UserId });
        });

        builder.Entity<UserPlanCancelationSurveyEntity>(b =>
        {
            b.ToTable("user_plan_cancelation_survey");
            b.ConfigureByConvention();

            b.Property(x => x.ReasonType).HasMaxLength(64);
            b.Property(x => x.ReasonText).HasMaxLength(1024);
            b.Property(x => x.Feedback).HasMaxLength(1024);

            b.HasIndex(x => new { x.UserId });
        });
    }
}
