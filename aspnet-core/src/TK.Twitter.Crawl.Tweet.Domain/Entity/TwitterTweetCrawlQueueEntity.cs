using Volo.Abp.Domain.Entities.Auditing;

namespace TK.Twitter.Crawl.Entity
{
    /// <summary>
    /// Lưu trữ dữ liệu thông tin các KOL sẽ thực hiện scan
    /// Bản ghi nào đã thực hiện hoàn tất quá trình crawl thì Ended = true
    /// </summary>
    public class TwitterTweetCrawlQueueEntity : FullAuditedEntity<long>
    {
        private const int MAX_TRY_ATTEMPT = 2;

        /// <summary>
        /// Acc để chạy crawl
        /// </summary>
        public string TwitterAccountId { get; set; }

        public string BatchKey { get; set; }

        /// <summary>
        /// User ID của KOL (Người đi follow dự án)
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Đánh dấu quá trình lấy dữ liệu từ Twitter đã hoàn tất
        /// </summary>
        public bool Ended { get; set; }

        /// <summary>
        /// Đánh dấu quá trình thực hiện crawl thành công
        /// </summary>
        public bool Successed { get; set; }

        /// <summary>
        /// Lưu lại số lần thực hiện crawl
        /// </summary>
        public int ProcessedAttempt { get; set; }

        /// <summary>
        /// Lưu lại số lần thực hiện crawl bị lỗi. Nếu quá số lần thì tự động chuyển Ended = true. Đồng thời lưu log để cảnh báo
        /// </summary>
        public int ErrorProcessedAttempt { get; set; }

        /// <summary>
        /// Ghi chú quá trình thực hiện
        /// </summary>
        public string Note { get; set; }

        public string CurrentCursor { get; set; }

        public string Tags { get; set; }

        public void UpdateProcessStatus(bool successed)
        {
            Successed = successed;

            // Các bản ghi đã đánh dấu là Ended r thì bỏ qua
            if (Ended)
            {
                return;
            }

            if (successed)
            {
                Ended = true;
            }
            else
            {
                if (ProcessedAttempt >= MAX_TRY_ATTEMPT)
                {
                    Ended = true;
                }
            }
        }
    }
}
