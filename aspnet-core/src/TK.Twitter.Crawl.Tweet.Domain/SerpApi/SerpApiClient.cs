using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SerpApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TK.Twitter.Crawl.Tweet.SerpApi
{
    public class SerpApiClient : ITransientDependency
    {
        const string API_KEY = "79a5f2e760477188d067de2c8f064a39721088bf363fbe3b1e6b4249d7a5bc93";
        private readonly ILogger<SerpApiClient> _logger;

        public SerpApiClient(ILogger<SerpApiClient> logger)
        {
            _logger = logger;
        }

        public async Task<(int, List<Article>)> SearchByKeywordAsync(string keyword, int start = 1, int num = 100)
        {
            Hashtable ht = new Hashtable();
            ht.Add("engine", "google");
            ht.Add("start", start.ToString());
            ht.Add("num", num.ToString());
            ht.Add("q", keyword);
            ht.Add("google_domain", "google.com");
            ht.Add("gl", "us");
            ht.Add("hl", "en");
            ht.Add("tbm", "nws");
            ht.Add("api_key", API_KEY);

            int totalResults = 0;
            List<Article> articles = new List<Article>();
            try
            {
                GoogleSearch search = new GoogleSearch(ht, API_KEY);
                JObject data = search.GetJson();
                JArray results = (JArray)data["news_results"];

                var searchInformation = data["search_information"];
                totalResults = searchInformation["total_results"].Value<int>();

                articles = new List<Article>();
                foreach (JObject result in results)
                {
                    var json = JsonConvert.SerializeObject(result);
                    var article = JsonConvert.DeserializeObject<Article>(json);
                    try
                    {
                        article.DateValue = ParseRelativeTime(article.Date);
                    }
                    catch { }
                    articles.Add(article);
                }
            }
            catch (SerpApiSearchException ex)
            {
                _logger.LogError(ex, "SerpApiSearchException");
            }
            return await Task.FromResult((totalResults, articles));

        }

        public static DateTime ParseRelativeTime(string relativeTime)
        {
            DateTime currentTime = DateTime.Now;
            string[] parts = relativeTime.Split(' ');

            if (parts.Length < 3)
                throw new ArgumentException("Invalid relative time format");

            int quantity = int.Parse(parts[0]);
            string unit = parts[1];

            switch (unit.ToLower())
            {
                case "second":
                case "seconds":
                    return currentTime.AddSeconds(-quantity);
                case "minute":
                case "minutes":
                    return currentTime.AddMinutes(-quantity);
                case "hour":
                case "hours":
                    return currentTime.AddHours(-quantity);
                case "day":
                case "days":
                    return currentTime.AddDays(-quantity);
                case "week":
                case "weeks":
                    return currentTime.AddDays(-quantity * 7);
                case "month":
                case "months":
                    return currentTime.AddMonths(-quantity);
                case "year":
                case "years":
                    return currentTime.AddYears(-quantity);
                default:
                    throw new ArgumentException("Invalid time unit");
            }
        }

        public class Article
        {
            public int Position { get; set; }
            public string Link { get; set; }
            public string Title { get; set; }
            public string Source { get; set; }
            public string Date { get; set; }
            public DateTime DateValue { get; set; }
            public string Snippet { get; set; }
            public string Thumbnail { get; set; }
        }
    }
}
