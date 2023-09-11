using Microsoft.EntityFrameworkCore;
using TK.TwitterAccount.Domain;
using TK.TwitterAccount.Domain.Entities;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace TK.TwitterAccount.EntityFrameworkCore
{
    [ConnectionStringName(TwitterAccountDbProperties.ConnectionStringName)]
    public class TwitterAccountDbContext : AbpDbContext<TwitterAccountDbContext>, ITwitterAccountDbContext
    {


        public TwitterAccountDbContext(DbContextOptions<TwitterAccountDbContext> options)
            : base(options)
        {

        }

        public DbSet<TwitterAccountEntity> TwitterAccountEntities { get; set; }

        public DbSet<TwitterAccountAPIEntity> TwitterAPIEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigureTwitterAccount();
        }
    }
}
