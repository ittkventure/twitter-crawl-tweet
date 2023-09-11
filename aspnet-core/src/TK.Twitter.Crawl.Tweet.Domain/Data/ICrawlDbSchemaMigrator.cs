using System.Threading.Tasks;

namespace TK.Twitter.Crawl.Data;

public interface ICrawlDbSchemaMigrator
{
    Task MigrateAsync();
}
