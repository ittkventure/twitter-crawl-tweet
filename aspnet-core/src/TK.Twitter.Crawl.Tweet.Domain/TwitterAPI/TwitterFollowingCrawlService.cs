using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Tweet.TwitterAPI.Dto.FollowingCrawl;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace TK.Twitter.Crawl.TwitterAPI
{
    public class TwitterFollowingCrawlService : ITransientDependency
    {
        protected HttpClient Client;

        public TwitterFollowingCrawlService(IConfiguration configuration)
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri(configuration.GetValue<string>("RemoteServices:TwitterFollowingCrawl:BaseUrl"));
        }

        public async Task<TwitterUserDto> GetByIdAsync(string id, string fields = null)
        {
            string url = $"/api/app/twitter/{id}/user-by-id?fields={fields}";
            var response = await Client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new UserFriendlyException("Có lỗi khi kết nối với Twitter Crawl API");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<TwitterUserDto>(content);
        }

        public async Task<TwitterUserDto> GetByUsernameAsync(string username, string fields = null)
        {
            string url = $"/api/app/twitter/user-by-username?username={username}&fields={fields}";
            var response = await Client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new UserFriendlyException("Có lỗi khi kết nối với Twitter Crawl API");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<TwitterUserDto>(content);
        }

        public async Task<List<TwitterUserDto>> GetUserByIdsAsync(string ids, string fields = null)
        {
            string url = $"/api/app/twitter/user-by-ids?ids={ids}&fields={fields}";
            var response = await Client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new UserFriendlyException("Có lỗi khi kết nối với Twitter Crawl API");
            }

            var content = await response.Content.ReadAsStringAsync();
            var payload = JsonHelper.Parse<List<TwitterUserDto>>(content);
            return payload;
        }

    }
}
