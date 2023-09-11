using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types.Enums;
using TK.Telegram.BackgroundJobs.Option;
using TK.Telegram.Domain.Entities;
using TK.Telegram.Domain.Service;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Telegram.BackgroundJobs
{
    public class TelegramBotSenderWorker : AsyncPeriodicBackgroundWorkerBase, ITransientDependency
    {
        public TelegramBotSenderWorker(
            AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
        {
            Timer.Period = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            var telegramBotSendingQueueRepository = workerContext.ServiceProvider.GetRequiredService<IRepository<TelegramBotSendingQueueEntity, long>>();
            var telegramBotSender = workerContext.ServiceProvider.GetRequiredService<ITelegramBotSender>();
            var clock = workerContext.ServiceProvider.GetRequiredService<IClock>();

            var now = clock.Now;
            var query = from q in await telegramBotSendingQueueRepository.GetQueryableAsync()
                        where q.Succeeded == false && q.Ended == false
                        select q;

            query = query.OrderBy(x => x.CreationTime).Take(5);

            var asyncExecuter = telegramBotSendingQueueRepository.AsyncExecuter;

            var queueItems = await asyncExecuter.ToListAsync(query);
            if (queueItems.IsNullOrEmpty())
            {
                return;
            }

            foreach (var item in queueItems)
            {
                bool succeeded = false;
                try
                {
                    await telegramBotSender.SendAsync(item.ChatId, item.TextContent, parseMode: (ParseMode)item.ParseMode);
                    succeeded = true;
                }
                catch (Exception ex)
                {
                    item.Note = ex.Message;
                    Logger.LogError(ex, "An error occurred while Telegram bot sending message");
                    succeeded = false;
                }

                item.UpdateResult(succeeded);
                await telegramBotSendingQueueRepository.UpdateAsync(item);
            }
        }
    }
}