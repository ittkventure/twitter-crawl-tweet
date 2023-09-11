using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.TwitterAPI;
using TK.Twitter.Crawl.TwitterAPI.Dto;
using TK.TwitterAccount.Domain;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class TestTwitterGetFollowing : ITransientDependency
    {
        private readonly TwitterAPIUserService _twitterAPIUserService;
        private readonly IClock _clock;
        private readonly TwitterAPIAuthService _twitterAPIAuthService;
        private readonly ITwitterAccountRepository _twitterAccountRepository;
        private readonly IRepository<TwitterCrawlAccountEntity, long> _twitterCrawlAccountRepository;

        public TestTwitterGetFollowing(TwitterAPIUserService twitterAPIUserService, IClock clock, TwitterAPIAuthService twitterAPIAuthService,
            ITwitterAccountRepository twitterAccountRepository,
            IRepository<TwitterCrawlAccountEntity, long> twitterCrawlAccountRepository)
        {
            _twitterAPIUserService = twitterAPIUserService;
            _clock = clock;
            _twitterAPIAuthService = twitterAPIAuthService;
            _twitterAccountRepository = twitterAccountRepository;
            _twitterCrawlAccountRepository = twitterCrawlAccountRepository;
        }

        public async Task Test()
        {
            var entries = new List<TwitterAPIEntryDto>();
            //var response = await GetFollowings("1445433077166182404", "Account_2", entries);
            var response = await GetFollowings("1108654113095479296", "Account_2", entries);

            //List<string> accs = new List<string> {
            //    //"Account_4",
            //    //"Account_5",
            //    //"Account_6",
            //    "Account_7",
            //    //"Account_8",
            //};

            //foreach (var item in accs)
            //{
            //    var crawlAccountInfo = await _twitterCrawlAccountRepository.FirstOrDefaultAsync(x => x.AccountId == item);
            //    if (crawlAccountInfo == null)
            //    {
            //        crawlAccountInfo = await _twitterAPIAuthService.CheckLogin(item);
            //    }
            //}


            try
            {
                var data = entries.Select(x => x.Content.ItemContent.UserResults.Result.Legacy.Entities);
            }
            catch
            {

            }

        }

        public async Task TestGetUser()
        {
            var response = await _twitterAPIUserService.GetUserByIdsAsync(new List<string> { "43716107" }, "Account_1");
        }

        private async Task<(bool, string)> GetFollowings(string userId, string accountId, List<TwitterAPIEntryDto> result, string cursor = null)
        {
            Console.WriteLine("Cursor: " + cursor);
            Task delay(TimeSpan timeSpan)
            {
                return Task.Delay(timeSpan);
            }

            TwitterAPIUserGetFollowingResponse response = null;
            try
            {
                response = await _twitterAPIUserService.GetFollowingAsync(userId, accountId, cursor: cursor);
                if (response.RateLimit > 0 || response.TooManyRequest)
                {
                    var subtract = response.RateLimitResetAt.Value.Subtract(_clock.Now);
                    if (response.RateLimitRemaining <= 1)
                    {
                        await delay(subtract);
                        response = await _twitterAPIUserService.GetFollowingAsync(userId, accountId, cursor: cursor);
                    }
                }
            }
            catch (BusinessException ex)
            {
                if (ex.Code == CrawlDomainErrorCodes.TwitterAuthorizationError)
                {
                    // chỉ cho login lại 1 lần
                    response = await _twitterAPIUserService.GetFollowingAsync(userId, accountId, requiredLogin: true, cursor: cursor);
                }
                else
                {
                    return (false, ex.Message);
                }
            }

            if (response?.Data?.User?.Result?.Typename == "UserUnavailable")
            {
                return (false, "User Unavailable");
            }

            var timelineAddEntries = response?.Data?.User?.Result?.Timeline?.Timeline?.Instructions?.FirstOrDefault(x => x.Type == "TimelineAddEntries");
            if (timelineAddEntries == null)
            {
                return (false, "Do not have Instructions type TimelineAddEntries");
            }

            var entries = timelineAddEntries.Entries;
            var users = entries.Where(x => x.EntryId.StartsWith("user"));
            if (users.IsEmpty())
            {
                return (true, "Succeeded");
            }

            result.AddRange(users);
            var timelineCusor = entries.FirstOrDefault(x => x.EntryId.StartsWith("cursor-bottom"));
            if (timelineCusor == null)
            {
                return (true, "Can not get cursor bottom");
            }

            cursor = timelineCusor.Content.Value;

            return await GetFollowings(userId, accountId, result, cursor);
        }
    }
}
