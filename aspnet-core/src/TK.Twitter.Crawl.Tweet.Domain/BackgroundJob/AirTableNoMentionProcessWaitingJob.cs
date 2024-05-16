using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet.AirTable;
using TK.Twitter.Crawl.Tweet.MemoryLock;
using TK.Twitter.Crawl.Tweet.TwitterAPI.Dto.FollowingCrawl;
using TK.Twitter.Crawl.TwitterAPI;
using TK.Twitter.Crawl.TwitterAPI.Dto;
using TK.TwitterAccount.Domain.Entities;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Jobs
{
    public class AirTableNoMentionProcessWaitingJobArg
    {

    }

    public class AirTableNoMentionProcessWaitingJob : AsyncBackgroundJob<AirTableNoMentionProcessWaitingJobArg>, ITransientDependency
    {
        private const string LOG_PREFIX = "[AirTableNoMentionProcessWaitingJob] ";
        public const int BATCH_SIZE = 10;

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<AirTableNoMentionWaitingProcessEntity, long> _airTableNoMentionWaitingProcessRepository;
        private readonly IRepository<LeadWaitingProcessEntity, long> _leadWaitingProcessRepository;
        private readonly IRepository<AirTableNoMentionEntity, long> _airTableNoMentionRepository;
        private readonly IRepository<TwitterUserEntity, long> _twitterUserRepository;
        private readonly IRepository<TwitterTweetEntity, long> _twitterTweetRepository;
        private readonly IRepository<TwitterTweetHashTagEntity, long> _twitterTweetHashTagRepository;
        private readonly AirTableNoMentionManager _airTableNoMentionManager;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly MemoryLockProvider _memoryLockProvider;
        private readonly TwitterAPIUserService _twitterAPIUserService;
        private readonly IRepository<TwitterAccountEntity, Guid> _twitterAccountRepository;
        private readonly IRepository<TwitterTweetUserMentionEntity, long> _twitterTweetUserMentionRepository;
        private readonly IRepository<TwitterUserStatusEntity, long> _twitterUserStatusRepository;
        private readonly IRepository<TwitterUserTypeEntity, long> _twitterUserTypeRepository;
        private readonly TwitterTweetCrawlJob _twitterTweetCrawlJob;

        Volo.Abp.Linq.IAsyncQueryableExecuter AsyncExecuter { get; set; }

        public AirTableNoMentionProcessWaitingJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<AirTableNoMentionWaitingProcessEntity, long> airTableNoMentionWaitingProcessRepository,
            IRepository<LeadWaitingProcessEntity, long> leadWaitingProcessRepository,
            IRepository<AirTableNoMentionEntity, long> airTableNoMentionRepository,
            IRepository<TwitterUserEntity, long> twitterUserRepository,
            IRepository<TwitterTweetEntity, long> twitterTweetRepository,
            IRepository<TwitterTweetHashTagEntity, long> twitterTweetHashTagRepository,
            AirTableNoMentionManager airTableNoMentionManager,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            MemoryLockProvider memoryLockProvider,
            TwitterAPIUserService twitterAPIUserService,
            IRepository<TwitterAccountEntity, Guid> twitterAccountRepository,
            IRepository<TwitterTweetUserMentionEntity, long> twitterTweetUserMentionRepository,
            IRepository<TwitterUserStatusEntity, long> twitterUserStatusRepository,
            IRepository<TwitterUserTypeEntity, long> twitterUserTypeRepository,
            TwitterTweetCrawlJob twitterTweetCrawlJob)
        {
            _backgroundJobManager = backgroundJobManager;
            _airTableNoMentionWaitingProcessRepository = airTableNoMentionWaitingProcessRepository;
            _leadWaitingProcessRepository = leadWaitingProcessRepository;
            _airTableNoMentionRepository = airTableNoMentionRepository;
            _twitterUserRepository = twitterUserRepository;
            _twitterTweetRepository = twitterTweetRepository;
            _twitterTweetHashTagRepository = twitterTweetHashTagRepository;
            _airTableNoMentionManager = airTableNoMentionManager;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _memoryLockProvider = memoryLockProvider;
            _twitterAPIUserService = twitterAPIUserService;
            _twitterAccountRepository = twitterAccountRepository;
            _twitterTweetUserMentionRepository = twitterTweetUserMentionRepository;
            _twitterUserStatusRepository = twitterUserStatusRepository;
            _twitterUserTypeRepository = twitterUserTypeRepository;
            _twitterTweetCrawlJob = twitterTweetCrawlJob;
            AsyncExecuter = _airTableNoMentionWaitingProcessRepository.AsyncExecuter;
        }

        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(AirTableNoMentionProcessWaitingJobArg args)
        {
            using (var handle = _memoryLockProvider.TryAcquireLock($"AirTableNoMentionProcessWaitingJob"))
            {
                if (handle == null)
                {
                    Logger.LogInformation(LOG_PREFIX + "Some process is running!!!");
                    return;
                }

                using var uow = _unitOfWorkManager.Begin();
                try
                {
                    var queueQuery = from input in await _airTableNoMentionWaitingProcessRepository.GetQueryableAsync()
                                     where input.Ended == false
                                     select input;

                    // xử lý theo chiều oldest -> lastest
                    queueQuery = queueQuery.OrderBy(x => x.CreationTime).Take(BATCH_SIZE);

                    var queues = await AsyncExecuter.ToListAsync(queueQuery);
                    if (queues.IsEmpty())
                    {
                        return;
                    }

                    var groups = queues.GroupBy(x => x.Action);

                    foreach (var group in groups)
                    {
                        switch (group.Key)
                        {
                            case "PUSH":
                            default:
                                await PushAction(group.ToList());
                                break;
                            case "PULL":
                                await PullAction(group.ToList());
                                break;
                        }
                    }

                    await uow.SaveChangesAsync();
                    await uow.CompleteAsync();
                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync();
                    Logger.LogError(ex, LOG_PREFIX + "An error occurred while create/update data Airtable");
                }
            }

            await _backgroundJobManager.EnqueueAsync(args);
        }

        private async Task PushAction(List<AirTableNoMentionWaitingProcessEntity> queues)
        {
            var tweets = await AsyncExecuter.ToListAsync(

                        from t in await _twitterTweetRepository.GetQueryableAsync()
                        where queues.Select(x => x.RefId).Contains(t.TweetId)
                        select t
                        );

            var tweetHashTags = await AsyncExecuter.ToListAsync(

                        from t in await _twitterTweetHashTagRepository.GetQueryableAsync()
                        where queues.Select(x => x.RefId).Contains(t.TweetId)
                        select t
                );

            foreach (var item in queues)
            {
                bool succeed = false;
                string error = null;
                try
                {
                    var tweet = tweets.FirstOrDefault(x => x.TweetId == item.RefId);
                    var noMention = await _airTableNoMentionRepository.InsertAsync(new AirTableNoMentionEntity()
                    {
                        UserId = null,
                        UserName = null,
                        UserProfileUrl = null,
                        UserScreenName = null,
                        UserType = null,
                        UserStatus = null,
                        TweetOwnerUserId = tweet.UserId,
                        TweetDescription = tweet.FullText,
                        MediaMentioned = tweet.UserScreenName,
                        Signals = item.Signals,
                        LastestSponsoredDate = tweet.CreatedAt,
                        LastestSponsoredTweetUrl = "https://twitter.com/_/status/" + tweet.TweetId,
                        NumberOfSponsoredTweets = 1,
                        DuplicateUrlCount = 0,
                        HashTags = tweetHashTags.Select(x => x.Text).JoinAsString(","),
                        LastestTweetId = tweet.TweetId,
                        MediaMentionedProfileUrl = "https://twitter.com/" + tweet.UserScreenName,
                        SignalDescription = null
                    });

                    (succeed, error) = await _airTableNoMentionManager.AddLeadAsync(noMention, user: null);

                    item.Succeed = succeed;
                    if (!succeed)
                    {
                        item.Note = error;
                    }
                }
                catch (Exception ex)
                {
                    item.Note = ex.ToString();
                }

                item.Ended = true;
                await _airTableNoMentionWaitingProcessRepository.UpdateAsync(item);
            }
        }

        private async Task PullAction(List<AirTableNoMentionWaitingProcessEntity> queues)
        {
            var noMentionItems = await AsyncExecuter.ToListAsync(
                        from t in await _airTableNoMentionRepository.GetQueryableAsync()
                        where queues.Select(x => x.RefId).Contains(t.RecordId)
                        select t
            );

            var currentAcc = await _twitterAccountRepository.FirstOrDefaultAsync(x => x.Enabled == true);

            var batchKey = $"NO_MENTION_{_clock.Now.ToString("yyyyMMddHHmm")}";
            foreach (var item in queues)
            {
                bool succeed = false;
                string error = null;
                try
                {
                    var noMentionItem = noMentionItems.FirstOrDefault(x => x.RecordId == item.RefId);
                    if (noMentionItem == null)
                    {
                        continue;
                    }

                    int count = 0;
                    TwitterUserDto user = null;
                    bool tooManyRequest = false;
                    while (true)
                    {
                        count++;
                        (tooManyRequest, user) = await GetTwitterUserAsync(item.Ref2, currentAcc);

                        if (tooManyRequest)
                        {
                            currentAcc = await _twitterAccountRepository.FirstOrDefaultAsync(x => x.Enabled == true && x.AccountId != currentAcc.AccountId);
                            (tooManyRequest, user) = await GetTwitterUserAsync(item.Ref2, currentAcc);
                            if (!tooManyRequest)
                            {
                                if (user == null)
                                {
                                    throw new Exception($"Can not get User Id of {item.Ref2}");
                                }
                            }
                        }
                        else
                        {
                            if (user == null)
                            {
                                throw new Exception($"Can not get User Id of {item.Ref2}");
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (count == 4)
                        {
                            break;
                        }
                    }

                    if (user == null)
                    {
                        throw new Exception($"Can not get User Id of {item.Ref2}");
                    }



                    // User đã được đồng bộ trước đấy nên bỏ qua
                    if (user.Id == noMentionItem.UserId)
                    {
                        continue;
                    }

                    // Trường hợp no mention đã được đồng bộ trước đó thì cần
                    //  - Xóa bỏ mention với tweet đó.
                    //  - Xóa signal với tweet đó
                    //  - Đưa vào danh sách đồng bộ lại lead
                    if (noMentionItem.UserId.IsNotEmpty())
                    {
                        var oldMentionedUserId = noMentionItem.UserId;
                        // tạm thời kệ chưa làm gì thêm :))
                    }

                    noMentionItem.UserId = user.Id;
                    noMentionItem.UserName = user.Name;
                    noMentionItem.UserScreenName = user.ScreenName;
                    noMentionItem.UserProfileUrl = "https://twitter.com/" + user.ScreenName;

                    await _airTableNoMentionRepository.UpdateAsync(noMentionItem);
                                        
                    var mention = await _twitterTweetUserMentionRepository.InsertAsync(new()
                    {
                        UserId = noMentionItem.UserId,
                        Name = noMentionItem.UserName,
                        ScreenName = noMentionItem.UserScreenName,
                        NormalizeName = noMentionItem.UserName.ToLower(),
                        NormalizeScreenName = noMentionItem.UserScreenName.ToLower(),
                        TweetId = noMentionItem.UserId,
                        TweetCreatedAt = noMentionItem.LastestSponsoredDate
                    });

                    var signals = noMentionItem.Signals.Split(",");
                    await _twitterTweetCrawlJob.ProcessSignals(signals, mention.UserId, mention.TweetId, batchKey);

                    succeed = true;

                    item.Succeed = succeed;
                    if (!succeed)
                    {
                        item.Note = error;
                    }
                }
                catch (Exception ex)
                {
                    item.Note = ex.ToString();
                }

                item.Ended = true;
                await _airTableNoMentionWaitingProcessRepository.UpdateAsync(item);
            }
        }

        public async Task<(bool, TwitterUserDto)> GetTwitterUserAsync(string url, TwitterAccountEntity currentAcc)
        {
            bool tooManyRequest = false;
            TwitterUserDto user = null;

            string screenName;
            try
            {
                screenName = GetQueryParamAfterSlash(url);
            }
            catch
            {
                screenName = GetQueryParam(url);
            }

            if (screenName.IsEmpty())
            {
                return (tooManyRequest, user);
            }

            if (currentAcc == null)
            {
                return (tooManyRequest, user);
            }

            try
            {
                string jsonContentString;
                (tooManyRequest, jsonContentString) = await GetUserFromTwitterService(screenName, currentAcc.AccountId);
                if (tooManyRequest)
                {
                    return (tooManyRequest, user);
                }

                var jsonContent = JObject.Parse(jsonContentString);
                var data = jsonContent["data"];
                var userData = data["user"];

                if (userData == null)
                {
                    return (tooManyRequest, user);
                }

                if (userData["result"] == null)
                {
                    return (tooManyRequest, user);
                }

                if (userData["result"]["__typename"].ParseIfNotNull<string>() == "UserUnavailable")
                {
                    // User is suspended. Không lấy đc data trả về
                    return (tooManyRequest, user);
                }

                if (userData["result"]["legacy"] == null)
                {
                    return (tooManyRequest, user);
                }

                user = new TwitterUserDto()
                {
                    Id = userData["result"]["rest_id"].ParseIfNotNull<string>(),
                    Name = userData["result"]["legacy"]["name"].ParseIfNotNull<string>(),
                    ScreenName = userData["result"]["legacy"]["screen_name"].ParseIfNotNull<string>(),
                    Description = userData["result"]["legacy"]["description"].ParseIfNotNull<string>(),
                    ProfileImageUrl = userData["result"]["legacy"]["profile_image_url_https"].ParseIfNotNull<string>(),
                };

                string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
                if (DateTime.TryParseExact(
                    userData["result"]["legacy"]["created_at"].ParseIfNotNull<string>(),
                    format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime createdAt))
                {
                    user.CreatedAt = createdAt;
                }

                return (tooManyRequest, user);
            }
            catch
            {
                return (tooManyRequest, user);
            }
        }

        static string GetQueryParamAfterSlash(string url)
        {
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            Uri uri = new Uri(url);
            string path = uri.AbsolutePath;
            int slashIndex = path.LastIndexOf('/');
            if (slashIndex >= 0 && slashIndex < path.Length - 1)
            {
                string param = path.Substring(slashIndex + 1);
                return param;
            }
            else
            {
                return "";
            }
        }

        static string GetQueryParam(string url)
        {
            // Sử dụng Regex để lấy giá trị của query parameter
            // Bỏ qua giá trị đằng sau dấu ?
            Regex regex = new Regex(@"/([^/?]+)");
            Match match = regex.Match(url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Trường hợp không tìm thấy
            return "Không tìm thấy";
        }

        private async Task<(bool, string)> GetUserFromTwitterService(string screenName, string accountId)
        {
            bool tooManyRequest = false;
            string jsonContent = null;

            Task delay(TimeSpan timeSpan)
            {
                return Task.Delay(timeSpan);
            }

            TwitterAPIUserGetUserResponse response = null;
            try
            {
                response = await _twitterAPIUserService.GetUserByScreenNameAsync(screenName, accountId);
                tooManyRequest = response.TooManyRequest;
                jsonContent = response.JsonContent;
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
                    response = await _twitterAPIUserService.GetUserByScreenNameAsync(screenName, accountId, requiredLogin: true);
                }
                else
                {
                    return (tooManyRequest, jsonContent);
                }
            }

            return (tooManyRequest, jsonContent);
        }
    }
}
