using Microsoft.Extensions.DependencyInjection;
using TK.Telegram.Domain;
using TK.Telegram.Domain.Shared;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace TK.Telegram.EntityFrameworkCore
{
    [DependsOn(
         typeof(AbpEntityFrameworkCoreModule),
         typeof(TelegramDomainModule),
         typeof(TelegramDomainSharedModule))]
    public class TelegramEntityFrameworkCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<TelegramDbContext>(options =>
            {
                //options.AddRepository<TwitterAccountEntity, EfCoreTwitterAccountRepository>();
            });
        }
    }
}