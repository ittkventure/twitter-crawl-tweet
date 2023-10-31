using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Tweet;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace TK.Twitter.Crawl.Controllers
{
    [RemoteService(Name = "CoinBase")]
    [Area("coin-base")]
    [Route("coin-base")]
    public class CoinBaseController : AbpControllerBase
    {
        public CoinBaseController(IConfiguration configuration, ICoinBaseWebhookAppService coinBaseWebhookAppService)
        {
            Configuration = configuration;
            CoinBaseWebhookAppService = coinBaseWebhookAppService;
        }

        public IConfiguration Configuration { get; }
        public ICoinBaseWebhookAppService CoinBaseWebhookAppService { get; }

        [Route("webhook")]
        [HttpPost]
        public async Task<string> ReceiveAlertFromCoinBase()
        {
            string originalData = string.Empty;

            // Đọc dữ liệu từ Body của yêu cầu
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                originalData = await reader.ReadToEndAsync();
            }

            if (originalData.IsEmpty())
            {
                return "payload_not_found";
            }

            if (!CoinBaseWebhookVerify(originalData))
            {
                return "unauthorized";
            }

            return await CoinBaseWebhookAppService.HandleAlert(originalData);
        }

        private bool CoinBaseWebhookVerify(string originalData)
        {
            bool b = Request.Headers.TryGetValue("X-CC-Webhook-Signature", out var signature);
            if (signature.IsEmpty())
            {
                return false;
            }

            string secretKey = "59ad9c1a-4474-4d14-a62b-f3475a8c9a41";

            if (!VerifySignature(originalData, secretKey, signature.ToString()))
            {
                return false;
            }

            return true;
        }

        private bool VerifySignature(string originalData, string secretKey, string signature)
        {
            // Tạo đối tượng SHA256 HMAC
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                // Tạo chữ ký từ dữ liệu gốc
                byte[] signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(originalData));

                // Chuyển đổi chữ ký sang dạng hex string
                string computedSignature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

                // So sánh signature tính toán được với signature đưa vào để xác thực
                return computedSignature == signature;
            }
        }
    }
}