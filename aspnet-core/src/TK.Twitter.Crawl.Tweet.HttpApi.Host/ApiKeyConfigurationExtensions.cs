﻿using Microsoft.AspNetCore.Builder;

namespace TK.Twitter.Crawl;

public static class ApiKeyConfigurationExtensions
{
    public static void UseApiKeyAuthorization(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseWhen(context => context.Request.Path.StartsWithSegments("/api/app/admin-tweet"), appBuilder =>
        {
            appBuilder.UseMiddleware<ApiKeyAuthorizationMiddleware>();
        });
    }
}