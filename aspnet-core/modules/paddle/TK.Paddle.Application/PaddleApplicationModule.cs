using TK.Paddle.Application.Contracts;
using TK.Paddle.Domain;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace TK.Paddle.Application
{
    [DependsOn(
        typeof(AbpDddApplicationModule),
        typeof(PaddleApplicationContractsModule),
        typeof(PaddleDomainModule),
        typeof(AbpAutoMapperModule)
        )]
    public class PaddleApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<PaddleApplicationModule>();
            });

            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<PaddleApplicationModule>();
            });
        }
    }
}