using TK.TwitterAccount.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace TK.TwitterAccount.Domain
{
    public interface ITwitterAccountRepository : IRepository<TwitterAccountEntity, Guid>
    {
        Task<TwitterAccountEntity> GetAccountWithAPICanRequestAsync(string key);
    }
}