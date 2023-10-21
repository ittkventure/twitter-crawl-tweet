using Volo.Abp.Settings;

namespace TK.Twitter.Crawl.Settings;

public class CrawlEmailSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        var smtpPassword = context.GetOrNull("Abp.Mailing.Smtp.Password");
        if (smtpPassword != null)
        {
            smtpPassword.IsEncrypted = false;
            smtpPassword.DefaultValue = "A12C544C6D1E25195ED3E4E1416ECBEF9DAE";
        }
    }
}
