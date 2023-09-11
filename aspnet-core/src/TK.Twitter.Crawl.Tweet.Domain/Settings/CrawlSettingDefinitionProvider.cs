using Volo.Abp.Settings;

namespace TK.Twitter.Crawl.Settings;

public class CrawlSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(CrawlSettings.MySetting1));
    }
}
