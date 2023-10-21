using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TK.Paddle.Client.APIService.Product;
using TK.Paddle.Client.APIService.Subscription;
using TK.Paddle.Client.Base;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace TK.Paddle.Client
{
    [DependsOn(typeof(AbpHttpClientModule))]
    public class PaddleClientModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            string paddleBaseUrl = configuration.GetValue<string>("RemoteServices:Paddle:BaseUrl");

            context.Services.AddHttpClient<IPaddleSubscriptionAPIService, PaddleSubscriptionAPIService>(client =>
            {
                client.BaseAddress = new Uri(paddleBaseUrl);
            }).AddHttpMessageHandler<PaddleClientHandler>();

            context.Services.AddHttpClient<IPaddleProductAPIService, PaddleProductAPIService>(client =>
            {
                client.BaseAddress = new Uri(paddleBaseUrl);
            }).AddHttpMessageHandler<PaddleClientHandler>();
        }
    }
}