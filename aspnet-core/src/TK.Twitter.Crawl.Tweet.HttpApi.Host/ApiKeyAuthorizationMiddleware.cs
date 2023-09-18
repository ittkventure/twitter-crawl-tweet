using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TK.Twitter.Crawl;

public class ApiKeyAuthorizationMiddleware : IMiddleware, ITransientDependency
{
    public ApiKeyAuthorizationMiddleware()
    {
    }

    public virtual async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
#if DEBUG
        await next(context);
#else
        var key = context.Request.Headers["X-API-KEY"];
        var secret = context.Request.Headers["X-API-SECRET"];

        bool verified = key.ToString().EqualsIgnoreCase("TKLABS_API_KEY") && secret == "SecretKey&(9";
        if (!verified)
        {
            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
        }
        else
        {
            await next(context);
        }
#endif
    }
}
