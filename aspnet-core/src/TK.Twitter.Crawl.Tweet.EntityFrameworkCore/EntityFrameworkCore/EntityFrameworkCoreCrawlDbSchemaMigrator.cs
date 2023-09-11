using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TK.Twitter.Crawl.Data;
using Volo.Abp.DependencyInjection;

namespace TK.Twitter.Crawl.EntityFrameworkCore;

public class EntityFrameworkCoreCrawlDbSchemaMigrator
    : ICrawlDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreCrawlDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the CrawlDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<CrawlDbContext>()
            .Database
            .MigrateAsync();
    }
}
