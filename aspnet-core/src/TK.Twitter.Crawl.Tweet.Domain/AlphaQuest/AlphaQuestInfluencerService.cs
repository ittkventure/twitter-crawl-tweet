using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TK.Twitter.Crawl.AlphaQuest.Dto;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace TK.Twitter.Crawl.AlphaQuest
{
    public class AlphaQuestInfluencerService : DomainService
    {
        private readonly string BaseUrl;

        public AlphaQuestInfluencerService(HttpClient httpClient, IConfiguration configuration)
        {
            HttpClient = httpClient;
            Configuration = configuration;

            BaseUrl = configuration.GetValue<string>("RemoteServices:AlphaQuest:BaseUrl");

            HttpClient.BaseAddress = new Uri(BaseUrl);

            HttpClient.DefaultRequestHeaders.Add("X-API-KEY", "beccdd7b1a6e4c97aa947461d314d096");
            HttpClient.DefaultRequestHeaders.Add("X-API-SECRET", "f1d3efc267794600afc0e77b8610624a");
        }

        public HttpClient HttpClient { get; }
        public IConfiguration Configuration { get; }

        public async Task<PagingResult<AlphaQuestTwitterInfluencerDto>> GetListAsync(int skipCount, int maxResultCount, string searchText = null, string sorting = null)
        {
            string url = BaseUrl + $"/api/cms/app/twitter-influencer";
            var param = new Dictionary<string, string>() {
                { "skipCount", skipCount.ToString() },
                { "maxResultCount", maxResultCount.ToString() },
            };

            if (searchText.IsNotEmpty())
            {
                param.Add("searchText", searchText);
            }

            if (sorting.IsNotEmpty())
            {
                param.Add("sorting", sorting);
            }

            url = QueryHelpers.AddQueryString(url, param);
            var response = await HttpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new UserFriendlyException("Có lỗi khi kết nối với AlphaQuest API");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<PagingResult<AlphaQuestTwitterInfluencerDto>>(content);
        }
    }
}
