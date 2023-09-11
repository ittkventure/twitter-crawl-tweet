using Microsoft.EntityFrameworkCore;
using TK.TwitterAccount.Domain;
using TK.TwitterAccount.Domain.Entities;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace TK.TwitterAccount.EntityFrameworkCore
{
    [ConnectionStringName(TwitterAccountDbProperties.ConnectionStringName)]
    public interface ITwitterAccountDbContext : IEfCoreDbContext
    {
        DbSet<TwitterAccountEntity> TwitterAccountEntities { get; }

        DbSet<TwitterAccountAPIEntity> TwitterAPIEntities { get; }
    }
}