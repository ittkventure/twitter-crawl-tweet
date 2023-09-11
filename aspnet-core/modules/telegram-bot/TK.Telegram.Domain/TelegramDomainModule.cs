using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TK.Telegram.Domain.Option;
using TK.Telegram.Domain.Shared;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace TK.Telegram.Domain
{
    [DependsOn(
        typeof(TelegramDomainSharedModule),
        typeof(AbpHttpClientModule)
        )]
    public class TelegramDomainModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            Configure<TelgramOptions>(configuration.GetSection(TelgramOptions.Telegram));

            var telegramOptions = configuration.GetSection(TelgramOptions.Telegram).Get<TelgramOptions>();
            if (!telegramOptions.BotToken.IsNullOrWhiteSpace())
            {
                context.Services.AddHttpClient("telegram_bot_client").AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    TelegramBotClientOptions options = new(telegramOptions.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });
            }
        }
    }
}