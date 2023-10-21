using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace TK.Paddle.HttpApi
{
    [DependsOn(
       typeof(AbpAspNetCoreMvcModule)
     )]
    public class PaddleHttpApiModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            PreConfigure<IMvcBuilder>(mvcBuilder =>
            {
                mvcBuilder.AddApplicationPartIfNotExists(typeof(PaddleHttpApiModule).Assembly);
            });
        }
    }
}