using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TK.Twitter.Crawl.Data;

/* This is used if database provider does't define
 * ICrawlDbSchemaMigrator implementation.
 */
public class NullCrawlDbSchemaMigrator : ICrawlDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
