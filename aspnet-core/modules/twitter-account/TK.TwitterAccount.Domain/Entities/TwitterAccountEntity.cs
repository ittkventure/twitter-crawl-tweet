using Volo.Abp.Domain.Entities.Auditing;

namespace TK.TwitterAccount.Domain.Entities
{
    /// <summary>
    /// Các tài khoản Twitter dùng để crawl dữ liệu
    /// </summary>
    public class TwitterAccountEntity : FullAuditedEntity<Guid>
    {
        /// <summary>
        /// ID định danh tài khoản hệ thống tự đặt
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Tên gợi nhớ. Để biết được là tài khoản nào
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Email để đăng nhập
        /// </summary>
        public string Email { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// API key trong tài liệu của Twitter. Cặp Key/Secret để lấy Bearer token
        /// </summary>
        public string APIKey { get; set; }

        /// <summary>
        /// API Key secret trong tài liệu của Twitter
        /// </summary>
        public string APIKeySecret { get; set; }

        /// <summary>
        /// Bearer token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Đang hoạt động
        /// </summary>
        public bool Enabled { get; set; }
    }
}
