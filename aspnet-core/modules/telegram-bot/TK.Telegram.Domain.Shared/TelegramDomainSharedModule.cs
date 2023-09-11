using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Volo.Abp.VirtualFileSystem;

namespace TK.Telegram.Domain.Shared
{
    [DependsOn(typeof(AbpValidationModule))]
    public class TelegramDomainSharedModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<TelegramDomainSharedModule>();
            });

            //Configure<AbpLocalizationOptions>(options =>
            //{
            //    options.Resources
            //        .Add<PaddleResource>("en")
            //        .AddBaseTypes(typeof(AbpValidationResource))
            //        .AddVirtualJson("/Localization/Paddle");
            //});

            //Configure<AbpExceptionLocalizationOptions>(options =>
            //{
            //    options.MapCodeNamespace("Paddle", typeof(PaddleResource));
            //});
        }
    }
}
