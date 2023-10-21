using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TK.Paddle.Domain.Entity;
using TK.Paddle.Domain.Eto;
using TK.Paddle.Domain.Shared;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace TK.Paddle.Domain.Payment
{
    public class PaddleWebhookHandleAlertHandler : IDistributedEventHandler<PaddleWebhookHandleAlertEto>, ITransientDependency
    {
        private const string LOG_PREFIX = "[PaddleWebhookAppService] ";

        private readonly IRepository<PaddleWebhookLogEntity, Guid> _paddleWebhookLogRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedEventBus _distributedEventBus;

        public PaddleWebhookHandleAlertHandler(
            IRepository<PaddleWebhookLogEntity, Guid> paddleWebhookLogRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedEventBus distributedEventBus,
            ILogger<PaddleWebhookHandleAlertHandler> logger
            )
        {
            _paddleWebhookLogRepository = paddleWebhookLogRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedEventBus = distributedEventBus;
            Logger = logger;
        }

        public ILogger<PaddleWebhookHandleAlertHandler> Logger { get; }

        public async Task HandleEventAsync(PaddleWebhookHandleAlertEto eventData)
        {            
            PaddleAfterWebhookLogAddedEto @event = null;
            switch (eventData.AlertName)
            {
                case PaddleWebhookConst.AlertName.SUBSCRIPTION_PAYMENT_SUCCEEDED:
                    try
                    {                        
                        using var uow = _unitOfWorkManager.Begin();
                        var hasExisted = await _paddleWebhookLogRepository.AnyAsync(x => x.AlertId == eventData.AlertId && x.AlertName == eventData.AlertName);
                        if (hasExisted)
                        {
                            return;
                        }

                        var entity = new PaddleWebhookLogEntity()
                        {
                            AlertId = eventData.AlertId,
                            AlertName = eventData.AlertName,
                            Raw = eventData.Raw
                        };

                        entity = await _paddleWebhookLogRepository.InsertAsync(entity, autoSave: true);
                        @event = new PaddleAfterWebhookLogAddedEto
                        {
                            AlertId = eventData.AlertId,
                            AlertName = eventData.AlertName,
                        };
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, LOG_PREFIX + ex.Message);
                    }
                    break;
                default:
                    break;
            }

            if (@event != null)
            {
                await _distributedEventBus.PublishAsync(@event);
            }
        }
    }
}
