using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.Text;
using System.Web;
using TK.Paddle.Application.Contracts;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace TK.Paddle.HttpApi
{
    [RemoteService(Name = "Paddle")]
    [Area("paddle")]
    [Route("paddle")]
    public class PaddleController : AbpControllerBase
    {
        public PaddleController(IPaddleWebhookAppService paddleWebhookAppService, IConfiguration configuration)
        {
            PaddleWebhookAppService = paddleWebhookAppService;
            Configuration = configuration;
        }

        public IPaddleWebhookAppService PaddleWebhookAppService { get; }
        public IConfiguration Configuration { get; }

        [Route("webhook")]
        [HttpPost]
        public async Task<string> ReceiveAlertFromPaddle()
        {
            if (!PaddleWebhookVerify())
            {
                return "unauthorized";
            }

            var form = Request.Form;
            if (form == null || form.Keys.IsEmpty())
            {
                return "form_invalid";
            }

            var alertName = form["alert_name"];
            if (alertName.IsEmpty())
            {
                return "alert_name_missing";
            }

            if (form.TryGetValue("subscription_plan_id", out var planId))
            {
                var handlePlanIds = Configuration.GetValue<string>("RemoteServices:Paddle:Webhook:HandlePlanIds").Split(",");
                if (!handlePlanIds.Contains(planId.ToString()))
                {
                    return "unhandle_plan_id";
                }
            }
            else
            {
                return "subscription_plan_id_invalid";
            }

            var dict = new Dictionary<string, string>();
            foreach (var itemkey in form.Keys)
            {
                dict.Add(itemkey, HttpUtility.UrlDecode(form[itemkey]));
            }

            var formUrlEncodedContent = new FormUrlEncodedContent(dict);
            var raw = await formUrlEncodedContent.ReadAsStringAsync();

            return await PaddleWebhookAppService.HandleAlert(long.Parse(form["alert_id"]), alertName, raw);
        }

        private bool PaddleWebhookVerify()
        {
            //// https://developer.paddle.com/webhook-reference/d8bbc4ae5cefa-security#ensure-webhooks-are-always-received-from-paddle
            //var allowedIps = Configuration.GetValue<string>("RemoteServices:Paddle:Webhook:AllowedIps");
            //var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
            //if (allowedIps.IsNotEmpty() && !allowedIps.Contains(remoteIpAddress.ToString()))
            //{
            //    return false;
            //}
            var formSignature = Request.Form["p_signature"];
            if (formSignature.IsEmpty())
            {
                return false;
            }

            byte[] signature = Convert.FromBase64String(formSignature.ToString());

            // https://vendors.paddle.com/public-key?_gl=1*33e2ls*_ga*MTE2NTk0MzQyOS4xNjc5NzMxMDA4*_ga_9XVE7HZLLZ*MTY3OTgyNjgxMi42LjEuMTY3OTgyOTkwOC41OC4wLjA.
            string publicKey = System.IO.File.ReadAllText("StaticData/paddle_public_key.pem");

            SortedDictionary<string, dynamic> padStuff = new SortedDictionary<string, dynamic>();
            foreach (var key in Request.Form.Keys)
            {
                if (key == "p_signature")
                {
                    continue;
                }

                padStuff.Add(key, Request.Form[key].ToString());
            }

            var serializer = new PhpSerializer();
            string payload = serializer.Serialize(padStuff);

            if (!VerifySignature(signature, payload, publicKey))
            {
                return false;
            }

            return true;
        }

        private bool VerifySignature(byte[] signatureBytes, string message, string pubKey)
        {
            StringReader newStringReader = new StringReader(pubKey);
            AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)new PemReader(newStringReader).ReadObject();
            ISigner sig = SignerUtilities.GetSigner("SHA1withRSA");
            sig.Init(false, publicKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            sig.BlockUpdate(messageBytes, 0, messageBytes.Length);
            return sig.VerifySignature(signatureBytes);
        }
    }
}