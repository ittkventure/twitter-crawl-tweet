using System.Collections.Generic;
using System;
using Volo.Abp.Domain.Entities.Auditing;
using System.Linq;

namespace TK.Twitter.Crawl.Entity
{
    /// <summary>
    /// Bảng lưu dữ liệu plan của user
    /// </summary>
    public class UserPlanEntity : FullAuditedAggregateRoot<Guid>
    {
        public UserPlanEntity()
        {
            UpgradeHistoryItems = new List<UserPlanUpgradeHistoryEntity>();
            PaddleSubscriptions = new List<UserPlanPaddleSubscriptionEntity>();
        }

        public UserPlanEntity(Guid id) : base(id)
        {
            UpgradeHistoryItems = new List<UserPlanUpgradeHistoryEntity>();
            PaddleSubscriptions = new List<UserPlanPaddleSubscriptionEntity>();
        }

        public Guid UserId { get; set; }

        public string PlanKey { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiredAt { get; set; }

        public int PaymentMethod { get; set; }

        public List<UserPlanPaddleSubscriptionEntity> PaddleSubscriptions { get; set; }

        public List<UserPlanUpgradeHistoryEntity> UpgradeHistoryItems { get; set; }

        public void AddHistory(UserPlanUpgradeHistoryEntity historyItem)
        {
            historyItem.UserId = UserId;
            UpgradeHistoryItems.Add(historyItem);
        }

        public void PaddleAddSubscription(long subscriptionId)
        {
            var current = PaddleSubscriptions.FirstOrDefault(x => x.IsCurrent);
            if (current == null)
            {
                PaddleSubscriptions.Add(new UserPlanPaddleSubscriptionEntity()
                {
                    UserId = UserId,
                    SubscriptionId = subscriptionId,
                    IsCurrent = true,
                });
            }
            else
            {
                var last = current;
                last.IsCurrent = false;

                PaddleSubscriptions.Add(new UserPlanPaddleSubscriptionEntity()
                {
                    UserId = UserId,
                    SubscriptionId = subscriptionId,
                    IsCurrent = true,
                });
            }
        }

        public void PaddleCancelSubsciption(long subscriptionId, DateTime? cancellationEffectiveDate)
        {
            var current = PaddleSubscriptions.FirstOrDefault(x => x.IsCurrent && x.SubscriptionId == subscriptionId);
            if (current != null)
            {
                current.IsCanceled = true;
                current.CancellationEffectiveDate = cancellationEffectiveDate;
            }
        }
    }
}
