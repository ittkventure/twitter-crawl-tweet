using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using Volo.Abp.DependencyInjection;

namespace TK.CoinGecko.Client.CoinGecko.Service
{
    public class CoinGeckoClientHandler : DelegatingHandler, ITransientDependency
    {
        private const int MAX_RETRIES = 2;

        private readonly IConfiguration _configuration;
        private readonly ILogger<CoinGeckoClientHandler> _logger;

        public CoinGeckoClientHandler(IConfiguration configuration, ILogger<CoinGeckoClientHandler> logger) : base()
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            if (!request.Headers.TryGetValues("x-cg-pro-api-key", out var value))
            {
                request.Headers.Add("x-cg-pro-api-key", _configuration.GetValue<string>("RemoteServices:CoinGecko:APIKey"));
            }

            for (int i = 0; i < MAX_RETRIES; i++)
            {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                var resultContent = await response.Content?.ReadAsStringAsync();
                if (resultContent.IsNotEmpty())
                {
                    _logger.LogError("[CGK Service]: " + resultContent);
                }

                // Nếu đã bị CoinGecko chặn do gọi quá số lượng request rồi thì thôi
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    continue;
                }
            }

            return response;
        }
    }
}
