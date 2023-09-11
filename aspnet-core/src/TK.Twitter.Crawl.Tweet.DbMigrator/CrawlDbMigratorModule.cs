using TK.Twitter.Crawl.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace TK.Twitter.Crawl.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(CrawlEntityFrameworkCoreModule),
    typeof(CrawlApplicationContractsModule)
    )]
public class CrawlDbMigratorModule : AbpModule
{

}
