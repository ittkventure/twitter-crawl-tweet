using Microsoft.Extensions.DependencyInjection;
using TK.Paddle.Domain;
using TK.Paddle.Domain.Shared;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace TK.Paddle.EntityFrameworkCore
{
    [DependsOn(
        typeof(AbpEntityFrameworkCoreModule),
        typeof(PaddleDomainModule),
        typeof(PaddleDomainSharedModule))]
    public class PaddleEntityFrameworkCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<PaddleDbContext>(options =>
            {
                //options.AddRepository<TwitterAccountEntity, EfCoreTwitterAccountRepository>();
            });
        }
    }
}