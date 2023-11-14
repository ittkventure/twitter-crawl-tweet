using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using TK.CoinGecko.Client.CoinGecko;
using TK.CoinGecko.Client.CoinGecko.Service;
using TK.Twitter.Crawl.Entity;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Numerics;
using Telegram.Bot.Types;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class TestCoinGecko : ITransientDependency
    {
        private readonly ICoinGeckoService _coinGeckoService;
        private readonly IRepository<CoinGeckoCoinEntity, long> _coinGeckoCoinRepository;
        private readonly IRepository<CoinGeckoCoinWaitingProcessEntity, long> _coinGeckoCoinWaitingProcessRepository;

        public TestCoinGecko(ICoinGeckoService coinGeckoService,
            IRepository<CoinGeckoCoinEntity, long> coinGeckoCoinRepository,
            IRepository<CoinGeckoCoinWaitingProcessEntity, long> coinGeckoCoinWaitingProcessRepository)
        {
            _coinGeckoService = coinGeckoService;
            _coinGeckoCoinRepository = coinGeckoCoinRepository;
            _coinGeckoCoinWaitingProcessRepository = coinGeckoCoinWaitingProcessRepository;
        }

        public async Task Test()
        {
            //var coins = await _coinGeckoService.GetAllCoins();

            //foreach (var item in coins)
            //{
            //    await _coinGeckoCoinRepository.InsertAsync(new CoinGeckoCoinEntity()
            //    {
            //        CoinId = item.Id,
            //        Name = item.Name,
            //        Symbol = item.Symbol,
            //    });
            //}

            var goingAddCoins = new List<string>()
            {
                "zarp-stablecoin",
                "turbobot",
                "truck",
                "titanx",
                "taho",
                "super-cycle",
                "salsa-liquid-multiversx",
                "promptide",
                "planet-hares",
                "physics",
                "orb-wizz-council",
                "naxion",
                "mintra",
                "megaton-finance-wrapped-toncoin",
                "iotec-finance",
                "horizon-protocol-zbnb",
                "grokdogex",
                "gambex",
                "flooring-protocol-microotherdeed",
                "flooring-protocol-micronakamigos",
                "flooring-protocol-microdegods",
                "flooring-protocol-microclonex",
                "bridged-wrapped-ether-fuse",
                "bridged-usdc-fuse",
                "aves"
            };

            var inserts = new List<CoinGeckoCoinEntity>();
            foreach (var item in goingAddCoins)
            {
                string jsonContent = null;
                try
                {
                    jsonContent = await _coinGeckoService.GetCoinDetailById(item);
                }
                catch (BusinessException ex)
                {
                    if (ex.Code == CoinGeckoQaCode.TooManyRequestError)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(70));
                        jsonContent = await _coinGeckoService.GetCoinDetailById(item);
                    }

                    if (ex.Code == CoinGeckoQaCode.NotFound)
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex + "An Error when Get coin detail data");
                }

                if (jsonContent != null)
                {
                    var entity = await _coinGeckoCoinRepository.FirstOrDefaultAsync(x => x.CoinId == item);
                    entity.JsonContent = jsonContent;
                    await _coinGeckoCoinRepository.UpdateAsync(entity);

                    await _coinGeckoCoinWaitingProcessRepository.InsertAsync(
                             new CoinGeckoCoinWaitingProcessEntity()
                             {
                                 CoinId = entity.CoinId,
                                 Action = "CREATE"
                             }
                    );
                }
            }
        }
    }
}
