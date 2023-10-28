using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;
using Volo.Abp.Validation;

namespace TK.Twitter.Crawl.Tweet
{
    public class CoinBaseWebhookAppService : ApplicationService, ICoinBaseWebhookAppService
    {
        private const string LOG_PREFIX = "[CoinBaseWebhookAppService] ";

        private readonly IRepository<CoinBaseWebhookLogEntity, Guid> _coinBaseWebhookLogRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IRepository<CoinBaseWebhookProcessEntity, long> _coinBaseWebhookProcessRepository;

        public CoinBaseWebhookAppService(
            IRepository<CoinBaseWebhookLogEntity, Guid> coinBaseWebhookLogRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedEventBus distributedEventBus,
            IRepository<CoinBaseWebhookProcessEntity, long> coinBaseWebhookProcessRepository)
        {
            _coinBaseWebhookLogRepository = coinBaseWebhookLogRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedEventBus = distributedEventBus;
            _coinBaseWebhookProcessRepository = coinBaseWebhookProcessRepository;
        }

        public async Task<string> HandleAlert(string raw)
        {
            CoinBaseAfterWebhookLogAddedEto @event = null;
            try
            {
                var jObject = JObject.Parse(raw);
                var eventId = jObject["id"].ParseIfNotNull<string>();

                using var uow = _unitOfWorkManager.Begin();
                var hasExisted = await _coinBaseWebhookLogRepository.AnyAsync(x => x.EventId == eventId);
                if (hasExisted)
                {
                    return "success";
                }

                string eventType = jObject["event"]?["type"].ParseIfNotNull<string>();

                var entity = new CoinBaseWebhookLogEntity()
                {
                    EventId = eventId,
                    EventType = eventType,
                    Raw = raw
                };

                entity = await _coinBaseWebhookLogRepository.InsertAsync(entity, autoSave: true);

                var processItem = await _coinBaseWebhookProcessRepository.InsertAsync(new CoinBaseWebhookProcessEntity()
                {
                    EventId = eventId,
                    EventType = eventType,
                }, autoSave: true);

                @event = new CoinBaseAfterWebhookLogAddedEto
                {
                    EventId = eventId,
                    EventType = eventType,
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LOG_PREFIX + ex.Message + $". Raw: {raw}");
            }

            if (@event != null)
            {
                await _distributedEventBus.PublishAsync(@event);
            }

            return "success";
        }
    }
}