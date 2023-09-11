using System.Text;
using TK.TwitterAccount.Domain.Dto;
using TK.TwitterAccount.Domain.Shared;
using Volo.Abp;

namespace TK.TwitterAccount.Domain.Clients
{
    public class TwitterAuthClient : TwitterClient
    {
        private const string GetBearerTokenUrl = "oauth2/token";

        public TwitterAuthClient(string apiKey, string apiSecret) : base()
        {
            var tokenCredential = $"{Uri.EscapeDataString(apiKey)}:{Uri.EscapeDataString(apiSecret)}";
            var tokenCredentialAsByteArr = Encoding.ASCII.GetBytes(tokenCredential);
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(tokenCredentialAsByteArr));
        }

        public async Task<TwitterAuthTokenResponse> GetTokenAsync()
        {
            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(Client.BaseAddress + GetBearerTokenUrl),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "Content-Type", "application/x-www-form-urlencoded;charset=UTF-8" },
                    { "Content-Length", "29" },
                    { "Accept-Encoding", "gzip" }
                })
            };

            var response = await Client.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new BusinessException(TwitterAccountDomainErrorCodes.ForbidenResource);
                }

                throw new BusinessException(TwitterAccountDomainErrorCodes.UnexpectedException);
            }

            string content = await response.Content.ReadAsStringAsync();
            var result = JsonHelper.Parse<TwitterAuthTokenResponse>(content);
            return result;
        }
    }
}
