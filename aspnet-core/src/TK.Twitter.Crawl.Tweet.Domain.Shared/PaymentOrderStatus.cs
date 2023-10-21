namespace TK.Twitter.Crawl;

public enum PaymentOrderStatus
{
    /// <summary>
    /// Hủy
    /// </summary>
    Canceled = -99,

    /// <summary>
    /// Hết hạn
    /// </summary>
    Expired = -50,

    /// <summary>
    /// khởi tạo
    /// </summary>
    Created = 0,

    /// <summary>
    /// Đang chờ thanh toán
    /// </summary>
    WaitingPayment = 10,

    /// <summary>
    /// Thanh toán 1 phần
    /// </summary>
    PatialPayment = 15,

    /// <summary>
    /// Hoàn tất thanh toán
    /// </summary>
    Completed = 20
}
