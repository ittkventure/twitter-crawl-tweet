namespace TK.Twitter.Crawl;

public static class CrawlDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */

    public const string PREFIX = "Lead3:";

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

    public static string UserNotFound = PREFIX + "00200";
    public static string UserEmailAlreadyVerified = PREFIX + "00201";
    public static string UserEmailNotProvided = PREFIX + "12002";
    public static string UserConfirmEmailTokenInvalid = PREFIX + "12003";
    public static string UserEmailConfirmed = PREFIX + "12004";
    public static string UserEmailNotConfirmed = PREFIX + "12005";


    public static string UserPlanPremiumPlanAlreadyExisted = PREFIX + "00400";
    public static string UserPlanPremiumPlanNotExisted = PREFIX + "00401";
    public static string UserPlanSubsciptionIdInvalid = PREFIX + "00402";
    public static string UserPlanHasUsedTrialingBefore = PREFIX + "00403";
    public static string UserPlanHasCancelPlanBefore = PREFIX + "00404";
    public static string UserPlanPaymentMethodInvalid = PREFIX + "00405";
    public static string UserPlanPaddleSubcriptionNotFound = PREFIX + "00406";
    public static string UserPlanJustPremiumPlanCanUseFeature = PREFIX + "00407";


    public static string PaymentOrderNotFound = PREFIX + "00500";
    public static string PaymentOrderNotBelongToUser = PREFIX + "00501";
    public static string PaymentCanNotParsePassthroughtData = PREFIX + "00502";
    public static string PaymentInvalidPlan = PREFIX + "00503";
    public static string PaymentPaymentMethodInvalid = PREFIX + "00504";
    public static string PaymentOrderAlreadyCompleted = PREFIX + "00505";

}
