using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using HangfireBasicAuthenticationFilter;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using OpenIddict.Validation.AspNetCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TK.Twitter.Crawl.BackgroundWorkers;
using TK.Twitter.Crawl.EntityFrameworkCore;
using TK.Twitter.Crawl.MultiTenancy;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs.Hangfire;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Timing;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace TK.Twitter.Crawl;
[DependsOn(
    typeof(CrawlHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(CrawlApplicationModule),
    typeof(CrawlEntityFrameworkCoreModule),
    //typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),

    typeof(AbpBackgroundJobsHangfireModule),
    typeof(AbpEventBusRabbitMqModule)
)]
public class CrawlHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("Crawl");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        ConfigureAuthentication(context);
        ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureConventionalControllers();
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
        ConfigureSwaggerServices(context, configuration);
        ConfigureHangfire(context, configuration);

        Configure<AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Utc;
        });

        context.Services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            var connection = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
            return new RedisDistributedSynchronizationProvider(connection.GetDatabase());
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
    }

    private void ConfigureBundles()
    {
        //Configure<AbpBundlingOptions>(options =>
        //{
        //    options.StyleBundles.Configure(
        //        LeptonXLiteThemeBundles.Styles.Global,
        //        bundle =>
        //        {
        //            bundle.AddFiles("/global-styles.css");
        //        }
        //    );
        //});
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());

            options.Applications["Angular"].RootUrl = configuration["App:ClientUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
        });
    }

    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<CrawlDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}TK.Twitter.Crawl.Tweet.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<CrawlDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}TK.Twitter.Crawl.Tweet.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<CrawlApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}TK.Twitter.Crawl.Tweet.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<CrawlApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}TK.Twitter.Crawl.Tweet.Application"));
            });
        }
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(CrawlApplicationModule).Assembly);
        });
    }

    private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"],
            new Dictionary<string, string>
            {
                    {"Crawl", "Crawl API"}
            },
            options =>
            {
                options.HideAbpEndpoints();
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Crawl API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration["App:CorsOrigins"]?
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => o.RemovePostFix("/"))
                        .ToArray() ?? Array.Empty<string>())
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureHangfire(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddHangfire(config =>
        {
            // MongoDB
            var mongoUrlBuilder = new MongoUrlBuilder(configuration.GetValue<string>("MongoDb:Hangfire"));
            var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());

            // Add Hangfire services. Hangfire.AspNetCore nuget required
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
            config.UseSimpleAssemblyNameTypeSerializer();
            config.UseRecommendedSerializerSettings();
            config.UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, new MongoStorageOptions
            {
                MigrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                    BackupStrategy = new CollectionMongoBackupStrategy()
                },
                // Fix warning: https://github.com/Hangfire-Mongo/Hangfire.Mongo/issues/300
                CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection,
                Prefix = "hangfire.mongo",
                CheckConnection = true
            });

            config.UseFilter(new AutomaticRetryAttribute { Attempts = 0 });
            config.UseColouredConsoleLogProvider();
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();
        //app.UseApiKeyAuthorization();

        if (MultiTenancyConsts.IsEnabled)
        {
            //app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crawl API");

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            c.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            c.OAuthScopes("Crawl");
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();

        var config = context.ServiceProvider.GetService<IConfiguration>();

        app.UseHangfireDashboard(options: new DashboardOptions()
        {
#if !DEBUG
            Authorization = new[]
            {
                new HangfireCustomBasicAuthenticationFilter{
                    User = config.GetValue<string>("HangfireSettings:UserName"),
                    Pass = config.GetValue<string>("HangfireSettings:Password")
                }
            }
#endif
        });

        app.UseConfiguredEndpoints();

        RecurringJob.RemoveIfExists(nameof(TwitterTweetCrawlWorker));
        if (config.GetValue<bool>("RecurringJobs:TwitterTweetCrawlWorker:Enable"))
        {
            RecurringJob.AddOrUpdate<TwitterTweetCrawlWorker>(nameof(TwitterTweetCrawlWorker), t => t.DoWorkAsync(), config.GetValue<string>("RecurringJobs:TwitterTweetCrawlWorker:CronExpression"));
        }

        RecurringJob.RemoveIfExists(nameof(LeadProcessWaitingWorker));
        if (config.GetValue<bool>("RecurringJobs:LeadProcessWaitingWorker:Enable"))
        {
            RecurringJob.AddOrUpdate<LeadProcessWaitingWorker>(nameof(LeadProcessWaitingWorker), t => t.DoWorkAsync(), config.GetValue<string>("RecurringJobs:LeadProcessWaitingWorker:CronExpression"));
        }

        RecurringJob.RemoveIfExists(nameof(AirTableProcessWaitingWorker));
        if (config.GetValue<bool>("RecurringJobs:AirTableProcessWaitingWorker:Enable"))
        {
            RecurringJob.AddOrUpdate<AirTableProcessWaitingWorker>(nameof(AirTableProcessWaitingWorker), t => t.DoWorkAsync(), config.GetValue<string>("RecurringJobs:AirTableProcessWaitingWorker:CronExpression"));
        }

        RecurringJob.RemoveIfExists(nameof(AirTableCheckDataWorker));
        if (config.GetValue<bool>("RecurringJobs:AirTableCheckDataWorker:Enable"))
        {
            RecurringJob.AddOrUpdate<AirTableCheckDataWorker>(nameof(AirTableCheckDataWorker), t => t.DoWorkAsync(), config.GetValue<string>("RecurringJobs:AirTableCheckDataWorker:CronExpression"));
        }

        RecurringJob.RemoveIfExists(nameof(AirTableCheckDataManualSourceWorker));
        if (config.GetValue<bool>("RecurringJobs:AirTableCheckDataManualSourceWorker:Enable"))
        {
            RecurringJob.AddOrUpdate<AirTableCheckDataManualSourceWorker>(nameof(AirTableCheckDataManualSourceWorker), t => t.DoWorkAsync(), config.GetValue<string>("RecurringJobs:AirTableCheckDataManualSourceWorker:CronExpression"));
        }

        RecurringJob.RemoveIfExists(nameof(AirTableManualSourceProcessWaitingWorker));
        if (config.GetValue<bool>("RecurringJobs:AirTableManualSourceProcessWaitingWorker:Enable"))
        {
            RecurringJob.AddOrUpdate<AirTableManualSourceProcessWaitingWorker>(nameof(AirTableManualSourceProcessWaitingWorker), t => t.DoWorkAsync(), config.GetValue<string>("RecurringJobs:AirTableManualSourceProcessWaitingWorker:CronExpression"));
        }
    }
}
