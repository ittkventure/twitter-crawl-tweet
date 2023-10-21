using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Web;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace TK.Paddle.Client.Base
{
    public class PaddleClientHandler : DelegatingHandler, ITransientDependency
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaddleClientHandler> _logger;

        public PaddleClientHandler(IConfiguration configuration, ILogger<PaddleClientHandler> logger) : base()
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var vendorId = _configuration.GetValue<string>("RemoteServices:Paddle:VendorId");
            var vendorAuthCode = _configuration.GetValue<string>("RemoteServices:Paddle:VendorAuthCode");

            Check.NotNullOrEmpty(vendorId, nameof(vendorId));
            Check.NotNullOrEmpty(vendorAuthCode, nameof(vendorAuthCode));

            var dataContent = new Dictionary<string, string>
                {
                    { "vendor_id", vendorId },
                    { "vendor_auth_code", vendorAuthCode },
                };

            if (request.Content == null)
            {
                request.Content = new FormUrlEncodedContent(dataContent);
            }
            else
            {
                // Tất cả các request của paddle đều dùng content type là form urlencoded
                var content = request.Content as FormUrlEncodedContent;
                var oldDataContent = await content.ReadAsStringAsync();
                if (oldDataContent.IsEmpty())
                {
                    request.Content = new FormUrlEncodedContent(dataContent);
                }
                else
                {
                    var pairs = oldDataContent.Split("&");
                    if (pairs.IsNotEmpty())
                    {
                        foreach (var item in pairs)
                        {
                            var nameValue = item.Split('=');
                            if (nameValue.Length == 2)
                            {
                                string name = HttpUtility.UrlDecode(nameValue[0]);
                                string value = HttpUtility.UrlDecode(nameValue[1]);
                                dataContent.Add(name, value);
                            }
                        }
                    }
                }

                request.Content = new FormUrlEncodedContent(dataContent);
            }

            return await base.SendAsync(request, cancellationToken); ;
        }
    }
}
