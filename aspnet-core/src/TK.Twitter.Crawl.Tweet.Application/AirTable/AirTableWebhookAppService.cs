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
    public class AirTableWebhookAppService : ApplicationService, IAirTableWebhookAppService
    {
        private const string LOG_PREFIX = "[AirTableWebhookAppService] ";

        private readonly IRepository<AirTableWebhookLogEntity, Guid> _airTableWebhookLogRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IRepository<AirTableWebhookProcessEntity, long> _airTableWebhookProcessRepository;

        public AirTableWebhookAppService(
            IRepository<AirTableWebhookLogEntity, Guid> airTableWebhookLogRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedEventBus distributedEventBus,
            IRepository<AirTableWebhookProcessEntity, long> airTableWebhookProcessRepository)
        {
            _airTableWebhookLogRepository = airTableWebhookLogRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedEventBus = distributedEventBus;
            _airTableWebhookProcessRepository = airTableWebhookProcessRepository;
        }

        public async Task<string> HandleAlert(string raw)
        {
            AirTableAfterWebhookLogAddedEto @event = null;
            try
            {
                var jObject = JObject.Parse(raw);

                var systemId = jObject["SystemID"].ParseIfNotNull<string>();
                string action = jObject["Action"].ParseIfNotNull<string>();

                using var uow = _unitOfWorkManager.Begin();

                var entity = new AirTableWebhookLogEntity()
                {
                    EventId = GuidGenerator.Create(),
                    SystemId = systemId,
                    Action = action,
                    Raw = raw
                };

                entity = await _airTableWebhookLogRepository.InsertAsync(entity, autoSave: true);

                var processItem = await _airTableWebhookProcessRepository.InsertAsync(new AirTableWebhookProcessEntity()
                {
                    EventId = entity.EventId,
                    SystemId = systemId,
                    Action = action,
                }, autoSave: true);

                @event = new AirTableAfterWebhookLogAddedEto
                {
                    EventId = entity.EventId,
                    SystemId = systemId,
                    Action = action,
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