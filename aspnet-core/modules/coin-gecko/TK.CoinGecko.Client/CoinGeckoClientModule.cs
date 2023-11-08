using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TK.CoinGecko.Client.CoinGecko.Service;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace TK.CoinGecko.Client
{
    [DependsOn(typeof(AbpHttpClientModule))]
    public class CoinGeckoClientModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();

            context.Services.AddHttpClient<ICoinGeckoService, CoinGeckoService>(client =>
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("RemoteServices:CoinGecko:BaseUrl"));
                client.Timeout = TimeSpan.FromSeconds(180);
            }).AddHttpMessageHandler<CoinGeckoClientHandler>();
        }

    }
}