using System.Threading.Tasks;
using TK.Twitter.Crawl.Tweet.GoogleNews;
using TK.Twitter.Crawl.Tweet.SerpApi;
using Volo.Abp.DependencyInjection;

namespace TK.Twitter.Crawl.Tweet.ConsoleApp.Test
{
    public class TestSerpApi : ITransientDependency
    {
        private readonly SerpApiClient _serpApiClient;
        private readonly GoogleNewsManager _googleNewsManager;

        public TestSerpApi(SerpApiClient serpApiClient,
            GoogleNewsManager googleNewsManager)
        {
            _serpApiClient = serpApiClient;
            _googleNewsManager = googleNewsManager;
        }

        public async Task RunAsync()
        {
            //await _googleNewsManager.CrawlAsync();
            try
            {
                await _googleNewsManager.SyncAirTableAsync();

            }
            catch (System.Exception)
            {

            }
        }
    }
}
