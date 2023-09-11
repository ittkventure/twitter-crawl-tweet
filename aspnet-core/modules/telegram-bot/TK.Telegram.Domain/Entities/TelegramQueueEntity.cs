using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Telegram.Domain.Entities
{
    public class TelegramBotSendingQueueEntity : AuditedEntity<long>
    {
        public string ChatId { get; set; }

        public string TextContent { get; set; }

        public int ParseMode { get; set; }

        public bool Ended { get; set; }

        public bool Succeeded { get; set; }

        public int AttemptCount { get; set; }

        public string Note { get; set; }

        public void UpdateResult(bool succeeded)
        {
            AttemptCount++;
            if (succeeded)
            {
                Succeeded = true;
                Ended = true;
                return;
            }

            if (AttemptCount >= 20)
            {
                Ended = true;
            }
        }
    }
}
