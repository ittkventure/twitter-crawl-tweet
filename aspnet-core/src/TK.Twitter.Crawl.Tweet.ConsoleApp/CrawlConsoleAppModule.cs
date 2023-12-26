using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using TK.Twitter.Crawl.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs.Hangfire;
using Volo.Abp.Modularity;
using Volo.Abp.Timing;

namespace TK.Twitter.Crawl.ConsoleApp;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(CrawlEntityFrameworkCoreModule),
    typeof(CrawlApplicationContractsModule),

    typeof(AbpBackgroundJobsHangfireModule)
    )]
public class CrawlConsoleAppModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        ConfigureHangfire(context, configuration);

        Configure<AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Utc;
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
}
