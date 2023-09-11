using System;
using System.Collections.Generic;
using System.Text;
using TK.Twitter.Crawl.Localization;
using Volo.Abp.Application.Services;

namespace TK.Twitter.Crawl;

/* Inherit your application services from this class.
 */
public abstract class CrawlAppService : ApplicationService
{
    protected CrawlAppService()
    {
        LocalizationResource = typeof(CrawlResource);
    }
}
