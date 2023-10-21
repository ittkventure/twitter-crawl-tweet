using Microsoft.Extensions.Logging;
using TK.Paddle.Application.Contracts;
using TK.Paddle.Domain.Entity;
using TK.Paddle.Domain.Eto;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace TK.Paddle.Application
{
    public class PaddleWebhookAppService : ApplicationService, IPaddleWebhookAppService
    {
        private const string LOG_PREFIX = "[PaddleWebhookAppService] ";

        private readonly IRepository<PaddleWebhookLogEntity, Guid> _paddleWebhookLogRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IRepository<PaddleWebhookProcessEntity, long> _paddleWebhookProcessRepository;

        public PaddleWebhookAppService(
            IRepository<PaddleWebhookLogEntity, Guid> paddleWebhookLogRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedEventBus distributedEventBus,
            IRepository<PaddleWebhookProcessEntity, long> paddleWebhookProcessRepository)
        {
            _paddleWebhookLogRepository = paddleWebhookLogRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedEventBus = distributedEventBus;
            _paddleWebhookProcessRepository = paddleWebhookProcessRepository;
        }

        public async Task<string> HandleAlert(long alertId, string alertName, string raw)
        {
            PaddleAfterWebhookLogAddedEto @event = null;
            try
            {
                using var uow = _unitOfWorkManager.Begin();
                var hasExisted = await _paddleWebhookLogRepository.AnyAsync(x => x.AlertId == alertId && x.AlertName == alertName);
                if (hasExisted)
                {
                    return "successed";
                }

                var entity = new PaddleWebhookLogEntity()
                {
                    AlertId = alertId,
                    AlertName = alertName,
                    Raw = raw
                };

                entity = await _paddleWebhookLogRepository.InsertAsync(entity, autoSave: true);

                var processItem = await _paddleWebhookProcessRepository.InsertAsync(new PaddleWebhookProcessEntity()
                {
                    AlertId = alertId,
                    AlertName = alertName
                }, autoSave: true);

                @event = new PaddleAfterWebhookLogAddedEto
                {
                    AlertId = alertId,
                    AlertName = alertName,
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LOG_PREFIX + ex.Message + $". Alert name: {alertName}.Alert ID: {alertId}");
            }

            if (@event != null)
            {
                await _distributedEventBus.PublishAsync(@event);
            }

            return "success";
        }
    }
}