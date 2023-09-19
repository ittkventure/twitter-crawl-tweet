using Microsoft.EntityFrameworkCore;
using TK.Telegram.Domain.Entities;
using TK.Telegram.EntityFrameworkCore;
using TK.Twitter.Crawl.Entity;
using TK.TwitterAccount.Domain.Entities;
using TK.TwitterAccount.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace TK.Twitter.Crawl.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class CrawlDbContext :
    AbpDbContext<CrawlDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext,
    ITwitterAccountDbContext,
    ITelegramDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public DbSet<TwitterCrawlAccountEntity> TwitterCrawlAccountEntities { get; set; }

    public DbSet<TwitterAccountEntity> TwitterAccountEntities { get; set; }

    public DbSet<TwitterAccountAPIEntity> TwitterAPIEntities { get; set; }

    public DbSet<TwitterTweetCrawlBatchEntity> TwitterTweetCrawlBatchEntities { get; set; }
    public DbSet<TwitterTweetCrawlQueueEntity> TwitterTweetCrawlQueueEntities { get; set; }

    public DbSet<TwitterInfluencerEntity> TwitterInfluencerEntities { get; set; }

    public DbSet<TwitterTweetEntity> TwitterTweetEntities { get; set; }
    public DbSet<TwitterTweetHashTagEntity> TwitterTweetHashTagEntities { get; set; }
    public DbSet<TwitterTweetMediaEntity> TwitterTweetMediaEntities { get; set; }
    public DbSet<TwitterTweetUserMentionEntity> TwitterTweetUserMentionEntities { get; set; }
    public DbSet<TwitterUserTypeEntity> TwitterUserTypeEntities { get; set; }
    public DbSet<TwitterUserStatusEntity> TwitterUserStatusEntities { get; set; }
    public DbSet<TwitterUserSignalEntity> TwitterUserSignalEntities { get; set; }
    public DbSet<TwitterTweetUrlEntity> TwitterTweetUrlEntities { get; set; }
    public DbSet<TwitterTweetSymbolEntity> TwitterTweetSymbolEntities { get; set; }
    public DbSet<TwitterTweetCrawlRawEntity> TwitterTweetCrawlTweetRawEntities { get; set; }

    public DbSet<TelegramBotSendingQueueEntity> TelegramBotSendQueueEntities { get; set; }

    public CrawlDbContext(DbContextOptions<CrawlDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();
        builder.ConfigureTwitterAccount();
        builder.ConfigureTelegramManagement();

        /* Configure your own tables/entities inside here */

        builder.Entity<TwitterInfluencerEntity>(b =>
        {
            b.ToTable("twitter_influencer");
            b.ConfigureByConvention();

            b.Property(x => x.Name).HasMaxLength(512);
            b.Property(x => x.ScreenName).HasMaxLength(512);
            b.Property(x => x.UserId).HasMaxLength(40);

            b.HasIndex(x => x.UserId);
        });

        builder.Entity<TwitterTweetCrawlBatchEntity>(b =>
        {
            b.ToTable("twitter_tweet_crawl_batch");
            b.ConfigureByConvention();

            b.Property(x => x.Key).HasMaxLength(40);

            b.HasIndex(x => x.CrawlTime);
            b.HasIndex(x => x.Key);
        });

        builder.Entity<TwitterTweetCrawlQueueEntity>(b =>
        {
            b.ToTable("twitter_tweet_crawl_queue");
            b.ConfigureByConvention();

            b.Property(x => x.BatchKey).HasMaxLength(40);
            b.Property(x => x.TwitterAccountId).HasMaxLength(40);
            b.Property(x => x.UserId).HasMaxLength(40);
            b.Property(x => x.CurrentCursor).HasMaxLength(64);
            b.HasIndex(x => new { x.BatchKey, x.UserId, x.Ended, x.TwitterAccountId });
        });

        builder.Entity<TwitterCrawlAccountEntity>(b =>
        {
            b.ToTable("twitter_crawl_account");
            b.ConfigureByConvention();

            b.Property(x => x.AccountId).HasMaxLength(40);
            b.Property(x => x.CookieCtZeroValue).HasMaxLength(1024);
            b.Property(x => x.GuestToken).HasMaxLength(1024);
            b.Property(x => x.Cookie).HasMaxLength(2650);
            b.HasIndex(x => new { x.AccountId });
        });

        builder.Entity<TwitterTweetEntity>(b =>
        {
            b.ToTable("twitter_tweet");
            b.ConfigureByConvention();

            b.Property(x => x.UserId).HasMaxLength(40);
            b.Property(x => x.UserName).HasMaxLength(512);
            b.Property(x => x.UserScreenName).HasMaxLength(512);
            b.Property(x => x.UserScreenNameNormalize).HasMaxLength(512);
            b.Property(x => x.TweetId).HasMaxLength(40);
            b.Property(x => x.Lang).HasMaxLength(25);
            b.Property(x => x.InReplyToScreenName).HasMaxLength(512);
            b.Property(x => x.InReplyToStatusId).HasMaxLength(56);
            b.Property(x => x.InReplyToUserId).HasMaxLength(40);
            b.Property(x => x.ConversationId).HasMaxLength(40);
            b.HasIndex(x => new { x.UserId });
            b.HasIndex(x => new { x.TweetId });
            b.HasIndex(x => new { x.CreatedAt });
        });

        builder.Entity<TwitterTweetCrawlRawEntity>(b =>
        {
            b.ToTable("twitter_tweet_crawl_raw");
            b.ConfigureByConvention();

            b.Property(x => x.TweetId).HasMaxLength(40);
            b.HasIndex(x => new { x.TweetId });
        });

        builder.Entity<TwitterTweetHashTagEntity>(b =>
        {
            b.ToTable("twitter_tweet_hash_tag");
            b.ConfigureByConvention();

            b.Property(x => x.TweetId).HasMaxLength(40);
            b.Property(x => x.Text).HasMaxLength(512);
            b.Property(x => x.NormalizeText).HasMaxLength(512);
            b.HasIndex(x => new { x.TweetId });
            b.HasIndex(x => new { x.NormalizeText });
        });

        builder.Entity<TwitterTweetMediaEntity>(b =>
        {
            b.ToTable("twitter_tweet_media");
            b.ConfigureByConvention();

            b.Property(x => x.TweetId).HasMaxLength(40);
            b.Property(x => x.MediaId).HasMaxLength(40);
            b.Property(x => x.Type).HasMaxLength(40);
            b.Property(x => x.DisplayUrl).HasMaxLength(512);
            b.Property(x => x.ExpandedUrl).HasMaxLength(512);
            b.Property(x => x.MediaUrlHttps).HasMaxLength(512);
            b.HasIndex(x => new { x.TweetId });
        });

        builder.Entity<TwitterTweetUserMentionEntity>(b =>
        {
            b.ToTable("twitter_tweet_user_mention");
            b.ConfigureByConvention();

            b.Property(x => x.TweetId).HasMaxLength(40);
            b.Property(x => x.UserId).HasMaxLength(40);
            b.Property(x => x.Name).HasMaxLength(512);
            b.Property(x => x.NormalizeName).HasMaxLength(512);
            b.Property(x => x.ScreenName).HasMaxLength(512);
            b.Property(x => x.NormalizeScreenName).HasMaxLength(512);
            b.HasIndex(x => new { x.TweetId });
            b.HasIndex(x => new { x.TweetId, x.CreationTime });
            b.HasIndex(x => new { x.TweetId, x.NormalizeScreenName });
            b.HasIndex(x => new { x.TweetId, x.ScreenName });
        });

        builder.Entity<TwitterTweetUrlEntity>(b =>
        {
            b.ToTable("twitter_tweet_url");
            b.ConfigureByConvention();

            b.Property(x => x.TweetId).HasMaxLength(40);
            b.Property(x => x.Url).HasMaxLength(512);
            b.Property(x => x.DisplayUrl).HasMaxLength(512);
            b.Property(x => x.ExpandedUrl).HasMaxLength(512);
            b.HasIndex(x => new { x.TweetId });
        });

        builder.Entity<TwitterTweetSymbolEntity>(b =>
        {
            b.ToTable("twitter_tweet_symbol");
            b.ConfigureByConvention();

            b.Property(x => x.TweetId).HasMaxLength(40);
            b.Property(x => x.SymbolText).HasMaxLength(512);
            b.HasIndex(x => new { x.TweetId });
        });

        builder.Entity<TwitterUserTypeEntity>(b =>
        {
            b.ToTable("twitter_user_type");
            b.ConfigureByConvention();

            b.Property(x => x.UserId).HasMaxLength(40);
            b.Property(x => x.Type).HasMaxLength(128);
            b.HasIndex(x => new { x.UserId });
        });

        builder.Entity<TwitterUserStatusEntity>(b =>
        {
            b.ToTable("twitter_user_status");
            b.ConfigureByConvention();

            b.Property(x => x.UserId).HasMaxLength(40);
            b.Property(x => x.Status).HasMaxLength(128);
            b.HasIndex(x => new { x.UserId });
        });

        builder.Entity<TwitterUserSignalEntity>(b =>
        {
            b.ToTable("twitter_user_signal");
            b.ConfigureByConvention();

            b.Property(x => x.UserId).HasMaxLength(40);
            b.Property(x => x.Signal).HasMaxLength(128);
            b.HasIndex(x => new { x.UserId });
            b.HasIndex(x => new { x.Signal });
        });

    }
}
