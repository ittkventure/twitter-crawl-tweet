using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace TK.Twitter.Crawl.Tweet.Slack
{
    public class SlackService : DomainService
    {
        private readonly HttpClient _httpClient;

        public SlackService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendAsync(string message)
        {
            var body = new SlackPostObject()
            {
                Text = message
            };

            var content = new StringContent(JsonHelper.Stringify(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://hooks.slack.com/services/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", content);
        }

        private class SlackPostObject
        {
            [JsonProperty("text")]
            public string Text { get; set; }
        }
    }
}
