using Microsoft.Extensions.DependencyInjection;
using TK.TwitterAccount.Domain.Entities;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace TK.TwitterAccount.EntityFrameworkCore
{
    [DependsOn(typeof(AbpEntityFrameworkCoreModule))]
    public class TwitterAccountEntityFrameworkCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<TwitterAccountDbContext>(options =>
            {
                options.AddRepository<TwitterAccountEntity, EfCoreTwitterAccountRepository>();
            });
        }
    }
}