using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TK.CoinGecko.Client.CoinGecko;
using TK.CoinGecko.Client.CoinGecko.Dto;
using TK.CoinGecko.Client.CoinGecko.Service;
using TK.Twitter.Crawl.Entity;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.BackgroundWorkers
{
    public class CoinGeckoSyncNewCoinWorker : ITransientDependency
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<CoinGeckoCoinEntity, long> _coinGeckoCoinRepository;
        private readonly IRepository<CoinGeckoCoinWaitingProcessEntity, long> _coinGeckoCoinWaitingProcessRepository;
        private readonly ICoinGeckoService _coinGeckoService;

        public IAsyncQueryableExecuter AsyncExecuter { get; set; }
        public ILogger<CoinGeckoSyncNewCoinWorker> Logger { get; }

        public CoinGeckoSyncNewCoinWorker(
            IBackgroundJobManager backgroundJobManager,
            IRepository<CoinGeckoCoinEntity, long> coinGeckoCoinRepository,
            IRepository<CoinGeckoCoinWaitingProcessEntity, long> coinGeckoCoinWaitingProcessRepository,
            ICoinGeckoService coinGeckoService,
            ILogger<CoinGeckoSyncNewCoinWorker> logger)
        {
            _backgroundJobManager = backgroundJobManager;
            _coinGeckoCoinRepository = coinGeckoCoinRepository;
            _coinGeckoCoinWaitingProcessRepository = coinGeckoCoinWaitingProcessRepository;
            _coinGeckoService = coinGeckoService;
            Logger = logger;
            AsyncExecuter = _coinGeckoCoinRepository.AsyncExecuter;
        }

        [UnitOfWork]
        public async Task DoWorkAsync()
        {
            var allCoins = await _coinGeckoService.GetAllCoins();

            var allCoinInDb = await AsyncExecuter.ToListAsync(
                from q in await _coinGeckoCoinRepository.GetQueryableAsync()
                select q.CoinId
                );

            var goingAddCoins = allCoins.Where(x => !allCoinInDb.Contains(x.Id));
            if (goingAddCoins.IsEmpty())
            {
                return;
            }

            var inserts = new List<CoinGeckoCoinEntity>();
            foreach (var item in goingAddCoins)
            {
                string jsonContent = null;
                try
                {
                    jsonContent = await _coinGeckoService.GetCoinDetailById(item.Id);
                }
                catch (BusinessException ex)
                {
                    if (ex.Code == CoinGeckoQaCode.TooManyRequestError)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(70));
                        jsonContent = await _coinGeckoService.GetCoinDetailById(item.Id);
                    }

                    if (ex.Code == CoinGeckoQaCode.NotFound)
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "An Error when Get coin detail data");
                }

                if (jsonContent != null)
                {
                    inserts.Add(new CoinGeckoCoinEntity()
                    {
                        CoinId = item.Id,
                        Symbol = item.Symbol,
                        Name = item.Name,
                        JsonContent = jsonContent
                    });
                }
            }

            if (inserts.IsNotEmpty())
            {
                await _coinGeckoCoinRepository.InsertManyAsync(inserts);
                await _coinGeckoCoinWaitingProcessRepository.InsertManyAsync(
                    inserts.Select(x => new CoinGeckoCoinWaitingProcessEntity()
                    {
                        CoinId = x.CoinId,
                        Action = "CREATE"
                    })
                    );
            }
        }
    }
}
