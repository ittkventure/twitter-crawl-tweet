using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Tweet;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace TK.Twitter.Crawl.Controllers
{
    [RemoteService(Name = "AirTable")]
    [Area("air-table")]
    [Route("air-table")]
    public class AirTableController : AbpControllerBase
    {
        public AirTableController(IConfiguration configuration, IAirTableWebhookAppService airTableWebhookAppService)
        {
            Configuration = configuration;
            AirTableWebhookAppService = airTableWebhookAppService;
        }

        public IConfiguration Configuration { get; }
        public IAirTableWebhookAppService AirTableWebhookAppService { get; }

        [Route("webhook")]
        [HttpPost]
        public async Task<string> Alert()
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

            var jObject = JObject.Parse(originalData);

            var checkCode = jObject["CheckCode"].ParseIfNotNull<string>();
            if (checkCode.IsEmpty())
            {
                return "check_code_not_found";
            }

            var secureCode = Configuration.GetValue<string>("RemoteServices:AirTable:WebhookSecureCode");
            if (secureCode != checkCode)
            {
                return "invalid";
            }

            return await AirTableWebhookAppService.HandleAlert(originalData);
        }
    }
}