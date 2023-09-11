using TK.TwitterAccount.Domain;
using TK.TwitterAccount.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TK.TwitterAccount.EntityFrameworkCore
{
    public class EfCoreTwitterAccountRepository : EfCoreRepository<ITwitterAccountDbContext, TwitterAccountEntity, Guid>, ITwitterAccountRepository
    {
        public EfCoreTwitterAccountRepository(IDbContextProvider<ITwitterAccountDbContext> dbContextProvider)
            : base(dbContextProvider)
        {

        }

        /// <summary>
        /// Thực hiện lấy thông tin tài khoản đang sẵn sàng để thực hiện việc truy cập API theo rate limit
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<TwitterAccountEntity> GetAccountWithAPICanRequestAsync(string key)
        {
            var context = await GetDbContextAsync();
            var query = from ta in context.TwitterAccountEntities
                        join api in context.TwitterAPIEntities on ta.AccountId equals api.AccountId
                        where api.Key == key && api.HasReachedLimit == false
                        select ta;

            return await AsyncExecuter.FirstOrDefaultAsync(query);
        }
    }
}
