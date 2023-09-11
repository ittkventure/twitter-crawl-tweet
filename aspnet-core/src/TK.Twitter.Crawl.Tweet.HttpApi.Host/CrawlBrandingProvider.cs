using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace TK.Twitter.Crawl;

[Dependency(ReplaceServices = true)]
public class CrawlBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "Crawl";
}
