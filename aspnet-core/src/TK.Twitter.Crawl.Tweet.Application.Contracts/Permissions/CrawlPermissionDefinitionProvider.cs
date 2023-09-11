using TK.Twitter.Crawl.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace TK.Twitter.Crawl.Permissions;

public class CrawlPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(CrawlPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(CrawlPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CrawlResource>(name);
    }
}
