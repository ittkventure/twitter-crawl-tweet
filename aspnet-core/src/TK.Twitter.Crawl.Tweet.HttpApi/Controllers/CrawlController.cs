using TK.Twitter.Crawl.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace TK.Twitter.Crawl.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class CrawlController : AbpControllerBase
{
    protected CrawlController()
    {
        LocalizationResource = typeof(CrawlResource);
    }
}
