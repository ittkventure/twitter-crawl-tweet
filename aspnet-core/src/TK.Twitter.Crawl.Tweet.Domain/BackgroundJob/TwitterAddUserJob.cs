using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.TwitterAPI;
using TK.Twitter.Crawl.TwitterAPI.Dto;
using TK.TwitterAccount.Domain;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Jobs
{
    public class TwitterAddUserJobArg
    {
        public List<string> UserIds { get; set; }
    }

    public class TwitterAddUserJob : AsyncBackgroundJob<TwitterAddUserJobArg>, ITransientDependency
    {
        private const string LOG_PREFIX = "[TwitterAddUserJob] ";

        private readonly IClock _clock;
        private readonly ITwitterAccountRepository _twitterAccountRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly TwitterAPIUserService _twitterAPIUserService;
        private readonly TwitterUserManager _twitterUserManager;

        public TwitterAddUserJob(
            IClock clock,
            ITwitterAccountRepository twitterAccountRepository,
            IUnitOfWorkManager unitOfWorkManager,
            TwitterAPIUserService twitterAPIUserService,
            TwitterUserManager twitterUserManager,
            ILogger<TwitterAddUserJob> logger)
        {
            _clock = clock;
            _twitterAccountRepository = twitterAccountRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _twitterAPIUserService = twitterAPIUserService;
            _twitterUserManager = twitterUserManager;
            Logger = logger;
        }

        public override async Task ExecuteAsync(TwitterAddUserJobArg args)
        {
            using var uow = _unitOfWorkManager.Begin();
            try
            {
                var crawlAcc = await _twitterAccountRepository.FirstOrDefaultAsync(x => x.Enabled == true);
                if (crawlAcc == null)
                {
                    throw new BusinessException(LOG_PREFIX + "All Crawl Account is disabled");
                }

                //API của Twitter giới hạn param
                const int BATCH_SIZE = 250;
                var batchs = args.UserIds.GetBatches(BATCH_SIZE);

                foreach (var batch in batchs)
                {
                    string responseContent = null;
                    try
                    {
                        var r = await GetUsers(batch, crawlAcc.AccountId);
                        responseContent = r;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, LOG_PREFIX + "An error occured while retrieving user data");
                    }

                    if (responseContent.IsNotEmpty())
                    {
                        var jsonContent = JObject.Parse(responseContent);
                        var data = jsonContent["data"];
                        var users = data["users"];

                        foreach (var item in users)
                        {
                            if (item["result"] == null)
                            {
                                continue;
                            }

                            if (item["result"]["__typename"].ParseIfNotNull<string>() == "UserUnavailable")
                            {
                                // User is suspended. Không lấy đc data trả về
                                continue;
                            }

                            if (item["result"]["legacy"] == null)
                            {
                                continue;
                            }

                            var cEntity = new TwitterUserEntity()
                            {
                                UserId = item["result"]["rest_id"].ParseIfNotNull<string>(),
                                Name = item["result"]["legacy"]["name"].ParseIfNotNull<string>(),
                                ScreenName = item["result"]["legacy"]["screen_name"].ParseIfNotNull<string>(),
                                Description = item["result"]["legacy"]["description"].ParseIfNotNull<string>(),
                                ProfileImageUrl = item["result"]["legacy"]["profile_image_url_https"].ParseIfNotNull<string>(),
                            };

                            string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
                            if (DateTime.TryParseExact(
                                item["result"]["legacy"]["created_at"].ParseIfNotNull<string>(),
                                format,
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out DateTime createdAt))
                            {
                                cEntity.CreatedAt = createdAt;
                            }

                            await _twitterUserManager.AddOrUpdateUserAsync(cEntity);
                        }

                        await uow.SaveChangesAsync();
                    }
                }
                await uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                await uow.RollbackAsync();
                Logger.LogError(ex, LOG_PREFIX + "An error occurred when add/update user");
                throw;
            }
        }

        private async Task<string> GetUsers(List<string> userIds, string accountId)
        {
            Task delay(TimeSpan timeSpan)
            {
                return Task.Delay(timeSpan);
            }

            TwitterAPIUserGetUserResponse response = null;
            try
            {
                response = await _twitterAPIUserService.GetUserByIdsAsync(userIds, accountId);
                if (response.RateLimit > 0 || response.TooManyRequest)
                {
                    var subtract = response.RateLimitResetAt.Value.Subtract(_clock.Now);
                    if (response.RateLimitRemaining == 1)
                    {
                        Logger.LogInformation(LOG_PREFIX + "Delay in " + subtract);
                        await delay(subtract);
                    }
                }
            }
            catch (BusinessException ex)
            {
                if (ex.Code == CrawlDomainErrorCodes.TwitterAuthorizationError)
                {
                    // chỉ cho login lại 1 lần
                    response = await _twitterAPIUserService.GetUserByIdsAsync(userIds, accountId, requiredLogin: true);
                }
                else
                {
                    return null;
                }
            }

            return response?.JsonContent;
        }
    }
}
