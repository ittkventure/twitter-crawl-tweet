using Volo.Abp.Domain.Entities.Auditing;

namespace TK.TwitterAccount.Domain.Entities
{
    public class TwitterAccountAPIEntity : FullAuditedEntity<long>
    {
        /// <summary>
        /// ID định danh tài khoản hệ thống tự đặt
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Dùng để Get dữ liệu và kiểm tra hạn mức
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Tên gợi nhớ của API
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Số lượng API tối đa trong 1 khoảng thời gian
        /// </summary>
        public int MaxRequestsPerWindowTime { get; set; }

        /// <summary>
        /// Khoảng thời gian tính theo đơn vị phút
        /// </summary>
        public int WindowTimeAsMinute { get; set; }

        /// <summary>
        /// Cờ đánh dấu API đã đạt hạn mức
        /// </summary>
        public bool HasReachedLimit { get; set; }
    }
}
