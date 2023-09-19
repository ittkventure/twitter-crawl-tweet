using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TK.Twitter.Crawl.Data;
using Serilog;
using Volo.Abp;
using Volo.Abp.Data;
using TK.Twitter.Crawl.TwitterAPI;
using Volo.Abp.Uow;
using System.Collections.Generic;
using TK.Twitter.Crawl.ConsoleApp.Test;
using TK.Twitter.Crawl.AlphaQuest;

namespace TK.Twitter.Crawl.ConsoleApp;

public class ConsoleAppHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IConfiguration _configuration;

    public ConsoleAppHostedService(IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var application = await AbpApplicationFactory.CreateAsync<CrawlConsoleAppModule>(options =>
        {
            options.Services.ReplaceConfiguration(_configuration);
            options.UseAutofac();
            options.Services.AddLogging(c => c.AddSerilog());
            options.AddDataMigrationEnvironment();
        }))
        {
            await application.InitializeAsync();

            var uowManager = application.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = uowManager.Begin();

            await application
                .ServiceProvider
                .GetRequiredService<TestReRunImportSignalData>().Test();
                //.GetRequiredService<ExportTweetReport>().Run();
                //.GetRequiredService<TestTwitterGetTweet>().Test();
            await uow.CompleteAsync();

            await application.ShutdownAsync();

            _hostApplicationLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
