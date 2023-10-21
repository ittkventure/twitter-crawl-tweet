using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TK.Paddle.Client.APIService.Product;
using TK.Paddle.Client.APIService.Product.Dto;
using TK.Paddle.Client.APIService.Subscription;
using TK.Paddle.Client.Base.Enum;
using Volo.Abp.Domain.Services;

namespace TK.Paddle.Domain.Test
{
    public class _20230325_APIProductTest : DomainService
    {
        private readonly IPaddleSubscriptionAPIService _paddleSubscriptionAPIService;
        private readonly IPaddleProductAPIService _paddleProductAPIService;

        public _20230325_APIProductTest(
            IPaddleSubscriptionAPIService paddleSubscriptionAPIService,
            IPaddleProductAPIService paddleProductAPIService)
        {
            _paddleSubscriptionAPIService = paddleSubscriptionAPIService;
            _paddleProductAPIService = paddleProductAPIService;
        }

        public async Task Test()
        {
            try
            {
                var listProductResponse = await _paddleProductAPIService.ListProductsAsync();
                var listSubscription = await _paddleSubscriptionAPIService.ListPlansAsync(planId: null);

                var product = listProductResponse.Response.Products[0];

                var response3 = await _paddleProductAPIService.GeneratePayLinkAsync(new PaddleProductGeneratePayLinkInput()
                {
                    //ProductId = product.Id,
                    ProductId = listSubscription.Response[0].Id,
                });

                var response2 = await _paddleProductAPIService.GeneratePayLinkAsync(
                    new PaddleProductGeneratePayLinkInput
                    {
                        ProductId = product.Id,
                        //WebhookUrl = "https://typedwebhook.tools/webhook/b0c3bb51-9a95-467c-b426-b1c2d6553bb9",
                        Title = "Subcription for AlphaQuest 1 month",
                        Prices = new List<string>
                        {
                            "USD:19.99",
                            "EUR:15.99"
                        },
                        ReturnUrl = "https://alphaquest.io/app",
                        RecurringPrices = new List<string>
                        {
                            "USD:19.99",
                            "EUR:15.99"
                        },
                        TrialDays = 7,
                        CustomMessage = "CustomMessage",
                        Discountable = PaddleYesNoEnum.No,
                        ImageUrl = "https://tk-storage.s3.ap-southeast-1.amazonaws.com/host/game/ReadyPlayerDAO_thumb_64x64_20220629112318.png",

                        QuantityVariable = PaddleYesNoEnum.Yes,
                        Quantity = 1,
                        Expires = new DateTime(2023, 3, 26).ToString("yyyy-MM-dd"),
                        CouponCode = null,

                        MarketingConsent = PaddleYesNoEnum.No,

                        CustomerEmail = "hoangphihai93@gmail.com",
                        CustomerCountry = "VN",
                        CustomerPostcode = "100000",
                        IsRecoverable = PaddleYesNoEnum.No,
                        Passthrough = "{\"user_id\": 98765, \"affiliation\": \"Acme Corp\"}",

                        //VatNumber = "EUR:15.99",
                        //VatCountry = "VN",
                        //VatPostcode = "100000",
                        //VatCity = "HN"
                    }
                );


                var a = 1;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
