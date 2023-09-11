namespace TK.Twitter.Crawl;

public static class CrawlDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */

    public const string PREFIX = "AlphaQuest:";

    /// <summary>
    /// This's common exception
    /// </summary>
    public static string UnexpectedException = PREFIX + "00000";

    public static string UnAuthorize = PREFIX + "00001";

    /// <summary>
    /// Tài khoản đang tạm khóa
    /// </summary>
    public static string AccountLockoutEnabled = PREFIX + "00002";

    /// <summary>
    /// This exception was throw when the input model not validated by system.
    /// </summary>
    public static string InputModelNotValidated = PREFIX + "00003";

    /// <summary>
    /// This exception was throw when the logic validated wrong
    /// </summary>
    public static string InsideLogicNotValidated = PREFIX + "00004";

    /// <summary>
    /// This exception was throw when we have duplicated resource
    /// </summary>
    public static string DuplicatedResource = PREFIX + "00005";

    /// <summary>
    /// This exception was throw when the resource not exists
    /// </summary>
    public static string NotFound = PREFIX + "00006";

    /// <summary>
    /// Error code cho các trường hợp không có quyền tác động lên resouce
    /// </summary>
    public static string ForbidenResourceCommon = PREFIX + "00007";

    /// <summary>
    /// This Exception was throw when the bussiness execute facing crash
    /// </summary>
    public static string InsideLogicError = PREFIX + "00008";

    public static string AnOnGoingProcessHasNotBeenCompleted = PREFIX + "00009";


    /// <summary>
    /// Lỗi khi Query dữ liệu của Twitter
    /// </summary>
    public static string TwitterQueryError = PREFIX + "00100";

    /// <summary>
    /// Lỗi khi Query dữ liệu của Twitter nhưng bị quá giới hạn Request
    /// </summary>
    public static string TwitterTooManyRequest = PREFIX + "00101";

    /// <summary>
    /// Lỗi khi không lấy được dữ liệu của API Activate khi Login Twitter
    /// </summary>
    public static string TwitterActivateError = PREFIX + "00102";

    /// <summary>
    /// GuestToken không hợp lệ
    /// </summary>
    public static string TwitterGuestTokenError = PREFIX + "00103";

    /// <summary>
    /// GuestToken không hợp lệ
    /// </summary>
    public static string TwitterFlowTokenError = PREFIX + "00104";

    /// <summary>
    /// Authorization: Denied by access control: unspecified reason
    /// </summary>
    public static string TwitterAuthorizationError = PREFIX + "00105";

    /// <summary>
    /// UnExpected Error
    /// </summary>
    public static string TwitterUnexpectedError = PREFIX + "00106";

}
