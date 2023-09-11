using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TK.Telegram.BackgroundJobs.Option;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;

namespace TK.Telegram.BackgroundJobs
{
    [DependsOn(typeof(AbpBackgroundJobsModule))]
    public class TelegramBackgroundJobsModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            Configure<TelegramBackgroundJobOptions>(configuration.GetSection(TelegramBackgroundJobOptions.TelegramBackgroundJob));
        }

        public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var options = context.ServiceProvider.GetRequiredService<IOptions<TelegramBackgroundJobOptions>>();
            if (options.Value.Enabled)
            {
                await context.AddBackgroundWorkerAsync<TelegramBotSenderWorker>();
            }
        }
    }
}