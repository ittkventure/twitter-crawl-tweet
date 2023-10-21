using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Localization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace TK.Twitter.Crawl;

/* Inherit your application services from this class.
 */
public abstract class CrawlAppService : ApplicationService
{
    protected IRepository<IdentityUser, Guid> UserRepository { get; set; }

    protected CrawlAppService()
    {
        LocalizationResource = typeof(CrawlResource);
    }

    protected async Task<bool> IsUserEmailConfirmed()
    {
        if (!CurrentUser.Id.HasValue)
        {
            return false;
        }

        if (UserRepository == null)
        {
            UserRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<IdentityUser, Guid>>();
        }

        var emailConfirmed = await UserRepository.AnyAsync(x => x.Id == CurrentUser.Id && x.EmailConfirmed == true);
        return emailConfirmed;
    }

    protected bool UserAuthorize()
    {
        return CurrentUser.Id.HasValue;
    }

    protected bool UserUnauthorize()
    {
        return !UserAuthorize();
    }

    protected void CheckLogin()
    {
        if (UserUnauthorize())
        {
            throw new UnauthorizedAccessException();
        }
    }
}
