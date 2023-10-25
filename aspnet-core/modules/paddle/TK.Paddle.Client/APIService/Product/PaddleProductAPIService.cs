using TK.Paddle.Client.APIService.Product.Dto;
using TK.Paddle.Client.APIService.Product.Response;
using TK.Paddle.Client.APIService.Subscription.Response;
using Volo.Abp;

namespace TK.Paddle.Client.APIService.Product
{
    public class PaddleProductAPIService : IPaddleProductAPIService
    {
        protected HttpClient Client { get; set; }

        private const string GENERATE_PAY_LINK_URL = "/2.0/product/generate_pay_link";
        private const string LIST_PRODUCTS_URL = "/2.0/product/get_products";

        public PaddleProductAPIService(HttpClient httpClient)
        {
            Client = httpClient;
        }

        #region Pay Links

        public async Task<PaddleProductGeneratePayLinkReponse> GeneratePayLinkAsync(PaddleProductGeneratePayLinkInput input)
        {
            var dataContent = new Dictionary<string, string>();
            if (input.ProductId.HasValue)
            {
                dataContent.Add("product_id", input.ProductId.ToString());
            }

            if (input.Title.IsNotEmpty())
            {
                dataContent.Add("title", input.Title);
            }

            if (input.WebhookUrl.IsNotEmpty())
            {
                dataContent.Add("webhook_url", input.WebhookUrl);
            }

            if (input.Prices.IsNotEmpty())
            {
                for (int i = 0; i < input.Prices.Count; i++)
                {
                    dataContent.Add($"prices[{i}]", input.Prices[i].ToString());
                }
            }

            if (input.RecurringPrices.IsNotEmpty())
            {
                for (int i = 0; i < input.RecurringPrices.Count; i++)
                {
                    dataContent.Add($"recurring_prices[{i}]", input.RecurringPrices[i].ToString());
                }
            }

            if (input.TrialDays.HasValue)
            {
                dataContent.Add("trial_days", input.TrialDays.ToString());
            }

            if (input.CustomMessage.IsNotEmpty())
            {
                Check.Length(input.CustomMessage, nameof(input.CustomMessage), 255);
                dataContent.Add("custom_message", input.CustomMessage);
            }

            if (input.CouponCode.IsNotEmpty())
            {
                Check.Length(input.CouponCode, nameof(input.CouponCode), 255, 5);
                dataContent.Add("coupon_code", input.CouponCode);
            }

            if (input.Discountable.HasValue)
            {
                dataContent.Add("discountable", ((int)input.Discountable).ToString());
            }

            if (input.ImageUrl.IsNotEmpty())
            {
                dataContent.Add("image_url", input.ImageUrl);
            }

            if (input.ReturnUrl.IsNotEmpty())
            {
                dataContent.Add("return_url", input.ReturnUrl);
            }

            if (input.QuantityVariable.HasValue)
            {
                dataContent.Add("quantity_variable", ((int)input.QuantityVariable).ToString());
            }

            if (input.Quantity.HasValue)
            {
                Check.Range(input.Quantity.Value, nameof(input.Quantity), 1, 100);
                dataContent.Add("quantity", input.Quantity.ToString());
            }

            if (input.Expires.IsNotEmpty())
            {
                if (!DateTime.TryParseExact(input.Expires, PaddleAPIServiceConst.DEFAULT_DATE_FORMAT, null, System.Globalization.DateTimeStyles.None, out var _))
                {
                    throw new ArgumentException(nameof(input.Expires) + " invalid format");
                }

                dataContent.Add("expires", input.Expires);
            }

            if (input.MarketingConsent.HasValue)
            {
                dataContent.Add("marketing_consent", ((int)input.MarketingConsent).ToString());
            }

            if (input.CustomerEmail.IsNotEmpty())
            {
                dataContent.Add("customer_email", input.CustomerEmail);
            }

            if (input.CustomerCountry.IsNotEmpty())
            {
                dataContent.Add("customer_country", input.CustomerCountry);
            }

            if (input.CustomerPostcode.IsNotEmpty())
            {
                dataContent.Add("customer_postcode", input.CustomerPostcode);
            }

            if (input.IsRecoverable.HasValue)
            {
                dataContent.Add("is_recoverable", ((int)input.IsRecoverable).ToString());
            }

            if (input.Passthrough.IsNotEmpty())
            {
                Check.Length(input.Passthrough, nameof(input.Passthrough), 1000);
                dataContent.Add("passthrough", input.Passthrough);
            }

            if (input.VatNumber.IsNotEmpty())
            {
                dataContent.Add("vat_number", input.VatNumber);
            }

            if (input.VatCompanyName.IsNotEmpty())
            {
                dataContent.Add("vat_company_name", input.VatCompanyName);
            }

            if (input.VatStreet.IsNotEmpty())
            {
                dataContent.Add("vat_street", input.VatStreet);
            }

            if (input.VatCity.IsNotEmpty())
            {
                dataContent.Add("vat_city", input.VatCity);
            }

            if (input.VatState.IsNotEmpty())
            {
                dataContent.Add("vat_state", input.VatState);
            }

            if (input.VatCountry.IsNotEmpty())
            {
                dataContent.Add("vat_country", input.VatCountry);
            }

            if (input.VatPostcode.IsNotEmpty())
            {
                dataContent.Add("vat_postcode", input.VatPostcode);
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + GENERATE_PAY_LINK_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleProductGeneratePayLinkReponse>(body);
        }

        #endregion

        #region Products

        public async Task<PaddleProductListProductsReponse> ListProductsAsync()
        {
            var dataContent = new Dictionary<string, string>();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Client.BaseAddress + LIST_PRODUCTS_URL),
                Content = new FormUrlEncodedContent(dataContent),
            };

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PaddleProductListProductsReponse>(body);
        }

        #endregion
    }
}
