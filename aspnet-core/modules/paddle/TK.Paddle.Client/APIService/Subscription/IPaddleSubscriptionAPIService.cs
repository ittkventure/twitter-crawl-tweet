using TK.Paddle.Client.APIService.Subscription.Response;
using TK.Paddle.Client.Base;
using TK.Paddle.Client.Base.Enum;

namespace TK.Paddle.Client.APIService.Subscription
{
    public interface IPaddleSubscriptionAPIService
    {

        #region Plans

        Task<PaddleSubscriptionListPlansResponse> ListPlansAsync(long? planId);

        #endregion

        #region Users

        Task<PaddleSubscriptionListUsersResponse> ListUsersAsync(long? subscriptionId = null, long? planId = null, string state = null, int? page = 1, int? resultsPerPage = 200);
        Task<PaddleSubscriptionUpdateUserResponse> UpdateUserAsync(long subscriptionId, int? quantity = null, string currency = null, decimal? recurringPrice = null, long? planId = null, bool? prorate = null, bool? billImmediately = null, bool? keepModifiers = null, string passthrough = null, bool? pause = null);
        Task<PaddleBaseResponse> CancelUserAsync(long subscriptionId);
        Task<PaddleSubscriptionUpdatePostcodeResponse> UpdatePostcodeAsync(long subscriptionId, string postCode);

        #endregion

        #region Modifiers

        Task<PaddleSubscriptionListModifiersResponse> ListModifiersAsync(long? subscriptionId = null, long? planId = null);
        Task<PaddleSubscriptionCreateModifierResponse> CreateModifierAsync(long subscriptionId, decimal modifierAmount, bool? modifierRecurring = null, string modifierDescription = null);
        Task<PaddleBaseResponse> DeleteModifierAsync(long modifierId);

        #endregion

        #region Payments

        Task<PaddleSubscriptionListPaymentsResponse> ListPaymentsAsync(long? subscriptionId = null, long? plan = null, PaddleYesNoEnum? isPaid = null, string from = null, string to = null, PaddleYesNoEnum? isOneOffCharge = null);
        Task<PaddleBaseResponse> ReschedulePaymentAsync(long paymentId, string date);

        #endregion


        #region Create one-off charge

        Task<PaddleSubscriptionCreateOneOffChargeResponse> CreateOneOffChargeAsync(long id, decimal amount, string chargeName);

        #endregion
    }
}
