using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TK.TwitterAccount.Domain.Entities;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace TK.TwitterAccount.EntityFrameworkCore
{
    public static class TwitterAccountDbContextModelBuilderExtensions
    {
        public static void ConfigureTwitterAccount([NotNull] this ModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.Entity<TwitterAccountEntity>(b =>
            {
                b.ToTable("twitter_account");
                b.ConfigureByConvention();

                b.Property(x => x.AccountId).HasMaxLength(256).IsRequired();
                b.Property(x => x.Name).HasMaxLength(256).IsRequired();
                b.Property(x => x.APIKey).HasMaxLength(256).IsRequired();
                b.Property(x => x.APIKeySecret).HasMaxLength(512).IsRequired();
                b.Property(x => x.AccessToken);

                b.HasIndex(x => new { x.AccountId });
            });

            builder.Entity<TwitterAccountAPIEntity>(b =>
            {
                b.ToTable("twitter_account_api");
                b.ConfigureByConvention();

                b.Property(x => x.Key).HasMaxLength(256).IsRequired();
                b.Property(x => x.Name).HasMaxLength(256).IsRequired();
                b.Property(x => x.AccountId).HasMaxLength(256).IsRequired();
                b.Property(x => x.Description);

                b.HasIndex(x => new { x.Key, x.AccountId });

                b.ApplyObjectExtensionMappings();
            });
        }
    }
}