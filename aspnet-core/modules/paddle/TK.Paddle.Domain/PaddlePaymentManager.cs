using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TK.Paddle.Client.APIService;
using TK.Paddle.Client.APIService.Product;
using TK.Paddle.Client.APIService.Product.Dto;
using TK.Paddle.Client.APIService.Product.Response;
using TK.Paddle.Domain.Dto;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace TK.Paddle.Domain
{
    public class PaddlePaymentManager : DomainService
    {
        private readonly IPaddleProductAPIService _paddleProductAPIService;
        private readonly IDistributedCache<PaymentUserGenerateLinkCacheItem, Guid> _distributedCache;

        public IConfiguration Configuration { get; }

        public string WebhookUrl => Configuration.GetValue<string>("RemoteServices:Paddle:Webhook:Url");
        public string ReturnUrlPattern => Configuration.GetValue<string>("RemoteServices:Paddle:ReturnUrl");
        public int ExpireDays => Configuration.GetValue<int>("RemoteServices:Paddle:ExpireDays");

        public PaddlePaymentManager(
            IPaddleProductAPIService paddleProductAPIService,
            IDistributedCache<PaymentUserGenerateLinkCacheItem, Guid> distributedCache,
            IConfiguration configuration)
        {
            _paddleProductAPIService = paddleProductAPIService;
            _distributedCache = distributedCache;
            Configuration = configuration;
        }

        public async Task<string> GeneratePaylink(Guid orderId, string userEmail, long planId)
        {
            var webhookUrl = WebhookUrl;
            var returnUrlPattern = ReturnUrlPattern;
            var expireDays = ExpireDays;

            var now = Clock.Now;

            var expireDate = now.Date;
            if (now.Hour > 20)
            {
                expireDate = expireDate.AddDays(expireDays);
            }

            string payLink = string.Empty;
            PaddleProductGeneratePayLinkReponse response = null;
            try
            {
                response = await _paddleProductAPIService.GeneratePayLinkAsync(new PaddleProductGeneratePayLinkInput()
                {
                    ProductId = planId,
                    ReturnUrl = string.Format(returnUrlPattern, orderId),
                    Expires = expireDate.ToString(PaddleAPIServiceConst.DEFAULT_DATE_FORMAT),
                    Passthrough = GetPassthrough(orderId),
                    CustomerEmail = userEmail,
                });

                payLink = response?.Response?.Url;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while generate link with Paddle");
                throw new UserFriendlyException("Generate link failed", PaddleDomainErrorCodes.PaymentGeneratePayLinkFailed);
            }

            if (response == null || !response.Success)
            {
                throw new UserFriendlyException(PaddleDomainErrorCodes.PaymentGeneratePayLinkFailed, "Generate link failed");
            }

            return payLink;
        }

        public string GetPassthrough(Guid orderId, Guid? userId = null)
        {
            string passthrough = $"order_id={orderId}";
            if (userId.HasValue)
            {
                passthrough += $"&user_id={userId.Value}";
            }

            return passthrough;
        }

        public Task<(Guid, Guid?)> GetPassthroughDataAsync(string raw)
        {
            Guid orderId = Guid.Empty;
            Guid? userId = null;

            if (raw.IsEmpty())
            {
                return Task.FromResult((orderId, userId));
            }

            var pairs = raw.Split("&");
            foreach (var p in pairs)
            {
                if (p.IsEmpty())
                {
                    continue;
                }

                var kv = p.Split('=');
                if (kv.Length != 2)
                {
                    continue;
                }

                var key = kv[0];
                var value = kv[1];

                if (key == "order_id")
                {
                    orderId = Guid.Parse(value);
                }
                else if (key == "user_id")
                {
                    userId = Guid.Parse(value);
                }
            }

            return Task.FromResult((orderId, userId));
        }

        public async Task<int> ReleaseLockGenerateLink(Guid userId)
        {
            await _distributedCache.RemoveAsync(userId);
            return 1;
        }

        private Task<PaymentUserGenerateLinkCacheItem> GetOrAddCacheAsync(Guid userId, int expireDays)
        {
            return _distributedCache.GetOrAddAsync(userId, () =>
            {
                return Task.FromResult(new PaymentUserGenerateLinkCacheItem()
                {
                    LastGenerateTime = Clock.Now
                });
            },
            () =>
            {
                return new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions()
                {
                    // Tự động giải phóng bộ nhớ sau 1 ngày + 1
                    AbsoluteExpiration = Clock.Now.AddDays(expireDays + 1)
                };
            });
        }

        private Task SetCacheAsync(Guid userId, Guid orderId, string paylink)
        {
            return _distributedCache.SetAsync(userId, new PaymentUserGenerateLinkCacheItem()
            {
                OrderId = orderId,
                LastGenerateTime = Clock.Now,
                PayLink = paylink
            });
        }

    }
}