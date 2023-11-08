using TK.CoinGecko.Client.CoinGecko.Dto;

namespace TK.CoinGecko.Client.CoinGecko.Service
{
    public interface ICoinGeckoService
    {
        Task<List<CoinGeckoCoinDto>> GetAllCoins();
        Task<string> GetCoinDetailById(string coinId);
    }
}