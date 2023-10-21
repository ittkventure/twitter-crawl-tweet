using TK.Paddle.Client.APIService.Subscription.Response;
using TK.Paddle.Client.Base;
using TK.Paddle.Client.Base.Enum;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace TK.Paddle.Client.APIService.Subscription
{
    public class PaddleSubscriptionAPIService : IPaddleSubscriptionAPIService, ITransientDependency
    {
        protected HttpClient Client { get; set; }

        private const string LIST_PLANS_URL = "/2.0/subscription/plans";

        private const string LIST_USERS_URL = "/2.0/subscription/users";
        private const string UPDATE_USER_URL = "/2.0/subscription/users/update";
        private const string CANCEL_USER_URL = "/2.0/subscription/users_cancel";
        private const string UPDATE_POSTCODE_URL = "/2.0/subscription/users/postcode";

        private const string LIST_MODIFIERS_URL = "/2.0/subscription/modifiers";
        private const string CREATE_MODIFIER_URL = "/2.0/subscription/modifiers/create";
        private const string DELETE_MODIFIER_URL = "/2.0/subscription/modifiers/delete";

        private const string LIST_PAYMENTS_URL = "/2.0/subscription/payments";
        private const string LIST_RESCHEDULE_PAYMENT_URL = "/2.0/subscription/payments_reschedule";

        private const string CREATE_ONE_OFF_CHARGE_URL = "/2.0/subscription/{0}/charge";

        public PaddleSubscriptionAPIService(HttpClient httpClient)
        {
            Client = httpClient;
        }

        #region Plans

        public async Task<PaddleSubscriptionListPlansResponse> ListPlansAsync(long? planId)
        {
            var dataContent = new Dictionary<string, string>();
            if (planId.HasValue)
            {
                dataContent.Add("plan", planId.ToString());
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + LIST_PLANS_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleSubscriptionListPlansResponse>(body);
        }

        #endregion

        #region Users

        public async Task<PaddleSubscriptionListUsersResponse> ListUsersAsync(
            long? subscriptionId = null,
            long? planId = null,
            string state = null,
            int? page = 1,
            int? resultsPerPage = 200)
        {
            var dataContent = new Dictionary<string, string>();

            if (planId.HasValue)
            {
                dataContent.Add("plan_id", planId.ToString());
            }

            if (subscriptionId.HasValue)
            {
                dataContent.Add("subscription_id", subscriptionId.ToString());
            }

            if (state.IsNotEmpty())
            {
                dataContent.Add("state", state);
            }

            if (page.HasValue)
            {
                dataContent.Add("page", page.ToString());
            }

            if (resultsPerPage.HasValue)
            {
                dataContent.Add("results_per_page", resultsPerPage.ToString());
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + LIST_USERS_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleSubscriptionListUsersResponse>(body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription you’re updating.</param>
        /// <param name="quantity">The new quantity to be applied to the subscription</param>
        /// <param name="currency">Optional, but required if setting recurring_price. The currency that the recurring price should be charged in. E.g. USD, GBP, EUR, etc. This must be the same as the currency of the existing subscription.</param>
        /// <param name="recurringPrice">The new recurring price per quantity unit to apply to the subscription. Please note this is a singular price, i.e 11.00.</param>
        /// <param name="planId">The new plan ID to move the subscription to.</param>
        /// <param name="prorate">Whether the change in subscription should be prorated.</param>
        /// <param name="billImmediately">If the subscription should bill for the next interval at the revised figures immediately.</param>
        /// <param name="keepModifiers">Retain the existing modifiers on the user subscription.</param>
        /// <param name="passthrough">Update the additional data associated with this subscription, like additional features, add-ons and seats. This will be included in all subsequent webhooks, and is often a JSON string of relevant data.</param>
        /// <param name="pause"></param>
        /// <returns></returns>
        public async Task<PaddleSubscriptionUpdateUserResponse> UpdateUserAsync(
            long subscriptionId,
            int? quantity = null,
            string currency = null,
            decimal? recurringPrice = null,
            long? planId = null,
            bool? prorate = null,
            bool? billImmediately = null,
            bool? keepModifiers = null,
            string passthrough = null,
            bool? pause = null)
        {
            Check.Range(subscriptionId, nameof(subscriptionId), 1);

            var dataContent = new Dictionary<string, string>
            {
                { "subscription_id", subscriptionId.ToString() }
            };

            if (quantity.HasValue)
            {
                Check.Range(quantity.Value, nameof(quantity), 1, 10000);
                dataContent.Add("quantity", quantity.ToString());
            }

            if (planId.HasValue)
            {
                dataContent.Add("plan_id", planId.ToString());
            }

            if (currency.IsNotEmpty())
            {
                dataContent.Add("currency", currency);
            }

            if (recurringPrice.HasValue)
            {
                dataContent.Add("recurring_price", recurringPrice.ToString());
            }

            if (prorate.HasValue)
            {
                dataContent.Add("prorate", prorate.ToString().ToLower());
            }

            if (billImmediately.HasValue)
            {
                dataContent.Add("bill_immediately", billImmediately.ToString().ToLower());
            }

            if (keepModifiers.HasValue)
            {
                dataContent.Add("keep_modifiers", keepModifiers.ToString().ToLower());
            }

            if (passthrough.IsNotEmpty())
            {
                dataContent.Add("passthrough", passthrough);
            }

            if (pause.HasValue)
            {
                dataContent.Add("pause", pause.ToString().ToLower());
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + UPDATE_USER_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleSubscriptionUpdateUserResponse>(body);
        }

        public async Task<PaddleBaseResponse> CancelUserAsync(long subscriptionId)
        {
            Check.Range(subscriptionId, nameof(subscriptionId), 1);

            var dataContent = new Dictionary<string, string>
            {
                { "subscription_id", subscriptionId.ToString() }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + CANCEL_USER_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleBaseResponse>(body);
        }

        public async Task<PaddleSubscriptionUpdatePostcodeResponse> UpdatePostcodeAsync(long subscriptionId, string postCode)
        {
            Check.NotNullOrEmpty(postCode, nameof(postCode));
            Check.Range(subscriptionId, nameof(subscriptionId), 1);

            var dataContent = new Dictionary<string, string>
            {
                { "subscription_id", subscriptionId.ToString() },
                { "postcode", postCode },
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + UPDATE_POSTCODE_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleSubscriptionUpdatePostcodeResponse>(body);
        }

        #endregion

        #region Modifiers

        public async Task<PaddleSubscriptionListModifiersResponse> ListModifiersAsync(long? subscriptionId = null, long? planId = null)
        {
            var dataContent = new Dictionary<string, string>();

            if (planId.HasValue)
            {
                dataContent.Add("plan_id", planId.ToString());
            }

            if (subscriptionId.HasValue)
            {
                dataContent.Add("subscription_id", subscriptionId.ToString());
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + LIST_MODIFIERS_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleSubscriptionListModifiersResponse>(body);
        }

        public async Task<PaddleSubscriptionCreateModifierResponse> CreateModifierAsync(
            long subscriptionId,
            decimal modifierAmount,
            bool? modifierRecurring = null,
            string modifierDescription = null)
        {
            Check.Range(subscriptionId, nameof(subscriptionId), 1);
            Check.Range(modifierAmount, nameof(modifierAmount), 0);
            Check.Length(modifierDescription, nameof(modifierDescription), 255);

            var dataContent = new Dictionary<string, string>
            {
                { "subscription_id", subscriptionId.ToString() },
                { "modifier_amount", modifierAmount.ToString() }
            };

            if (modifierDescription.IsNotEmpty())
            {
                dataContent.Add("modifier_description", modifierDescription);
            }

            if (modifierRecurring.HasValue)
            {
                dataContent.Add("modifier_recurring", modifierRecurring.ToString().ToLower());
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + CREATE_MODIFIER_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleSubscriptionCreateModifierResponse>(body);
        }

        public async Task<PaddleBaseResponse> DeleteModifierAsync(long modifierId)
        {
            Check.Range(modifierId, nameof(modifierId), 1);

            var dataContent = new Dictionary<string, string>
            {
                { "modifier_id", modifierId.ToString() },
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + DELETE_MODIFIER_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleBaseResponse>(body);
        }

        #endregion

        #region Payments

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="plan"></param>
        /// <param name="isPaid">Payment is paid (0 = No, 1 = Yes)</param>
        /// <param name="from">Payments starting after the date specified (date in format YYYY-MM-DD)</param>
        /// <param name="to">Payments ending the day before the date specified (date in format YYYY-MM-DD)</param>
        /// <param name="isOneOffCharge"></param>
        /// <returns>Non-recurring payments created from the  (0 = No, 1 = Yes)</returns>
        public async Task<PaddleSubscriptionListPaymentsResponse> ListPaymentsAsync(
            long? subscriptionId = null,
            long? plan = null,
            PaddleYesNoEnum? isPaid = null,
            string from = null,
            string to = null,
            PaddleYesNoEnum? isOneOffCharge = null)
        {
            var dataContent = new Dictionary<string, string>();
            if (subscriptionId.HasValue)
            {
                dataContent.Add("subscription_id", subscriptionId.ToString());
            }

            if (plan.HasValue)
            {
                dataContent.Add("plan", plan.ToString());
            }

            if (isPaid.HasValue)
            {
                dataContent.Add("is_paid", ((int)isPaid).ToString());
            }

            if (isOneOffCharge.HasValue)
            {
                dataContent.Add("is_one_off_charge", ((int)isOneOffCharge).ToString());
            }

            if (from.IsNotEmpty())
            {
                dataContent.Add("from", from);
            }

            if (to.IsNotEmpty())
            {
                dataContent.Add("to", to);
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + LIST_PAYMENTS_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleSubscriptionListPaymentsResponse>(body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentId">The upcoming subscription payment ID. This can be obtained by calling the <see cref="ListPaymentsAsync(long?, long?, bool?, string, string, int?)"/>>.</param>
        /// <param name="date">The date (in format YYYY-MM-DD) you want to move the payment to.</param>
        /// <returns></returns>
        public async Task<PaddleBaseResponse> ReschedulePaymentAsync(long paymentId, string date)
        {
            Check.Range(paymentId, nameof(paymentId), 1);
            Check.NotNullOrWhiteSpace(date, nameof(date));

            var dataContent = new Dictionary<string, string>() {
                { "payment_id", paymentId.ToString()},
                { "date", date },
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + LIST_RESCHEDULE_PAYMENT_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleSubscriptionListPaymentsResponse>(body);
        }

        #endregion


        #region One-off Charge

        public async Task<PaddleSubscriptionCreateOneOffChargeResponse> CreateOneOffChargeAsync(long id, decimal amount, string chargeName)
        {
            Check.Range(id, nameof(id), 1);
            Check.Range(amount, nameof(amount), 0);
            Check.NotNullOrWhiteSpace(chargeName, nameof(chargeName));
            Check.Length(chargeName, nameof(chargeName), 50);

            var dataContent = new Dictionary<string, string>() {
                { "amount", amount.ToString() },
                { "chargeName", chargeName },
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + string.Format(CREATE_ONE_OFF_CHARGE_URL, id)),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleSubscriptionCreateOneOffChargeResponse>(body);
        }

        #endregion
    }
}
