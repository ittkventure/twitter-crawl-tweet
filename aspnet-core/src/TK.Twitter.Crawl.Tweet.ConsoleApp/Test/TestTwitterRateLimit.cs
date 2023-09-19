using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TK.Twitter.Crawl.TwitterAPI;
using Volo.Abp.DependencyInjection;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class TestTwitterRateLimit : ITransientDependency
    {
        private readonly TwitterAPIUserService _twitterAPIUserService;

        public TestTwitterRateLimit(TwitterAPIUserService twitterAPIUserService)
        {
            _twitterAPIUserService = twitterAPIUserService;
        }

        public async Task Test()
        {
            var response = await _twitterAPIUserService.GetFollowingAsync("1476074281763233794", "Account_3");
            var response1 = await _twitterAPIUserService.GetUserByIdsAsync(new List<string> { "1523751438" }, "Account_2");
        }
    }
}
