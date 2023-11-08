using System.ComponentModel.DataAnnotations;
using System.Net;
using TK.CoinGecko.Client.CoinGecko.Dto;
using Volo.Abp;

namespace TK.CoinGecko.Client.CoinGecko.Service
{
    public class CoinGeckoService : ICoinGeckoService
    {
        public CoinGeckoService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public async Task<string> GetCoinDetailById(string coinId)
        {
            var url = $"coins/{coinId}?tickers=false&market_data=false&community_data=false&developer_data=false&sparkline=false";

            var response = await HttpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new BusinessException(CoinGeckoQaCode.TooManyRequestError);
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new BusinessException(CoinGeckoQaCode.NotFound);
                }

                throw new BusinessException(CoinGeckoQaCode.UnExpectedException);
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<CoinGeckoCoinDto>> GetAllCoins()
        {
            var url = $"coins/list";
            var response = await HttpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new BusinessException(CoinGeckoQaCode.TooManyRequestError);
                }

                throw new BusinessException(CoinGeckoQaCode.UnExpectedException);
            }

            var resultContent = await response.Content.ReadAsStringAsync();
            var coins = JsonHelper.Parse<List<CoinGeckoCoinDto>>(resultContent);
            return coins;
        }
    }
}