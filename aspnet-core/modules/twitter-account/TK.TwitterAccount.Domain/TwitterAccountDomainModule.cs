using TK.TwitterAccount.Domain.Shared;
using Volo.Abp.Modularity;

namespace TK.TwitterAccount.Domain
{
    [DependsOn(
        typeof(TwitterAccountDomainSharedModule)
    )]
    public class TwitterAccountDomainModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {


        }
    }

}