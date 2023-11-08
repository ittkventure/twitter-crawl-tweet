using Medallion.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
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
    public class CoinGeckoProcessWaitingJobArg
    {

    }

    public class CoinGeckoProcessWaitingJob : AsyncBackgroundJob<CoinGeckoProcessWaitingJobArg>, ITransientDependency
    {
        private const string LOG_PREFIX = "[CoinGeckoProcessWaitingJob] ";
        public const int BATCH_SIZE = 10;

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<CoinGeckoCoinEntity, long> _coinGeckoCoinEntityRepository;
        private readonly IRepository<TwitterUserSignalEntity, long> _twitterUserSignalRepository;
        private readonly IRepository<CoinGeckoCoinWaitingProcessEntity, long> _coinGeckoCoinWaitingProcessRepository;
        private readonly IRepository<LeadAnotherSourceEntity, long> _leadAnotherSourceRepository;
        private readonly IRepository<LeadWaitingProcessEntity, long> _leadWaitingProcessRepository;
        private readonly IRepository<TwitterUserTypeEntity, long> _twitterUserTypeRepository;
        private readonly IRepository<TwitterUserStatusEntity, long> _twitterUserStatusRepository;
        private readonly IRepository<TwitterAccountEntity, Guid> _twitterAccountRepository;
        private readonly TwitterAPIUserService _twitterAPIUserService;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly MemoryLockProvider _memoryLockProvider;
        private readonly TwitterFollowingCrawlService _twitterFollowingCrawlService;
        private readonly TwitterUserManager _twitterUserManager;

        public CoinGeckoProcessWaitingJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<CoinGeckoCoinEntity, long> coinGeckoCoinEntityRepository,
            IRepository<TwitterUserSignalEntity, long> twitterUserSignalRepository,
            IRepository<CoinGeckoCoinWaitingProcessEntity, long> coinGeckoCoinWaitingProcessRepository,
            IRepository<LeadAnotherSourceEntity, long> leadAnotherSourceRepository,
            IRepository<LeadWaitingProcessEntity, long> leadWaitingProcessRepository,
            IRepository<TwitterUserTypeEntity, long> twitterUserTypeRepository,
            IRepository<TwitterUserStatusEntity, long> twitterUserStatusRepository,
            IRepository<TwitterAccountEntity, Guid> twitterAccountRepository,
            TwitterAPIUserService twitterAPIUserService,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            MemoryLockProvider memoryLockProvider,
            TwitterFollowingCrawlService twitterFollowingCrawlService,
            TwitterUserManager twitterUserManager)
        {
            _backgroundJobManager = backgroundJobManager;
            _coinGeckoCoinEntityRepository = coinGeckoCoinEntityRepository;
            _twitterUserSignalRepository = twitterUserSignalRepository;
            _coinGeckoCoinWaitingProcessRepository = coinGeckoCoinWaitingProcessRepository;
            _leadAnotherSourceRepository = leadAnotherSourceRepository;
            _leadWaitingProcessRepository = leadWaitingProcessRepository;
            _twitterUserTypeRepository = twitterUserTypeRepository;
            _twitterUserStatusRepository = twitterUserStatusRepository;
            _twitterAccountRepository = twitterAccountRepository;
            _twitterAPIUserService = twitterAPIUserService;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _memoryLockProvider = memoryLockProvider;
            _twitterFollowingCrawlService = twitterFollowingCrawlService;
            _twitterUserManager = twitterUserManager;
        }

        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(CoinGeckoProcessWaitingJobArg args)
        {
            using (var handle = _memoryLockProvider.TryAcquireLock($"CoinGeckoProcessWaitingJob"))
            {
                if (handle == null)
                {
                    Logger.LogInformation(LOG_PREFIX + "Some process is running!!!");
                    return;
                }

                using var uow = _unitOfWorkManager.Begin();
                try
                {
                    var asyncExecuter = _coinGeckoCoinEntityRepository.AsyncExecuter;

                    var queueQuery = from input in await _coinGeckoCoinWaitingProcessRepository.GetQueryableAsync()
                                     where input.Ended == false
                                     select input;

                    // xử lý theo chiều oldest -> lastest
                    queueQuery = queueQuery.OrderBy(x => x.CreationTime).Take(BATCH_SIZE);

                    var queues = await asyncExecuter.ToListAsync(queueQuery);
                    if (queues.IsEmpty())
                    {
                        return;
                    }

                    var currentAcc = await _twitterAccountRepository.FirstOrDefaultAsync(x => x.Enabled == true);

                    var coins = await _coinGeckoCoinEntityRepository.GetListAsync(x => queues.Select(q => q.CoinId).Contains(x.CoinId));

                    foreach (var item in queues)
                    {
                        try
                        {
                            var coin = coins.FirstOrDefault(x => x.CoinId == item.CoinId);
                            if (coin == null)
                            {
                                throw new Exception($"Record not found");
                            }

                            switch (item.Action)
                            {
                                case "CREATE":
                                    var jobject = JObject.Parse(coin.JsonContent);
                                    var links = jobject["links"];
                                    var screenName = links["twitter_screen_name"].ParseIfNotNull<string>();
                                    var lastUpdated = jobject["last_updated"].ParseIfNotNull<DateTime?>();
                                    if (links == null || screenName.IsEmpty())
                                    {

                                    }
                                    else
                                    {
                                        var user = await GetTwitterUserAsync(screenName.Trim(), currentAcc);
                                        if (user == null)
                                        {
                                            throw new Exception($"Can not get User Id of {screenName}");
                                        }

                                        await _twitterUserManager.AddOrUpdateUserAsync(new TwitterUserEntity()
                                        {
                                            UserId = user.Id,
                                            ScreenName = user.ScreenName,
                                            Name = user.Name,
                                            Description = user.Description,
                                            CreatedAt = user.CreatedAt,
                                            ProfileImageUrl = user.ProfileImageUrl,
                                        }, autoSave: true);

                                        await _leadAnotherSourceRepository.InsertAsync(new LeadAnotherSourceEntity()
                                        {
                                            RefId = item.CoinId,
                                            Source = CrawlConsts.Signal.Source.COIN_GECKO,
                                            UserId = user.Id,
                                            UserScreenName = user.ScreenName,
                                            UserName = user.Name,
                                            Signals = CrawlConsts.Signal.JUST_LISTED_IN_COINGECKO,
                                            MediaMentioned = "Coingecko",
                                            SignalUrl = "https://www.coingecko.com/en/new-cryptocurrencies",
                                            Description = "Just listed in Coingecko",
                                            UpdatedAt = lastUpdated ?? coin.CreationTime
                                        });

                                        await _twitterUserSignalRepository.InsertAsync(new TwitterUserSignalEntity()
                                        {
                                            UserId = user.Id,
                                            Source = CrawlConsts.Signal.Source.COIN_GECKO,
                                            Signal = CrawlConsts.Signal.JUST_LISTED_IN_COINGECKO,
                                            RefId = item.CoinId
                                        });

                                        // source từ coingecko thì k phân loại user type

                                        // nếu không có thì thêm mới
                                        var userStatus = await _twitterUserStatusRepository.FirstOrDefaultAsync(x => x.UserId == user.Id);
                                        if (userStatus == null)
                                        {
                                            await _twitterUserStatusRepository.InsertAsync(new TwitterUserStatusEntity()
                                            {
                                                UserId = user.Id,
                                                Status = "New",
                                                IsUserSuppliedValue = false,
                                            }, autoSave: true);
                                        }

                                        await _leadWaitingProcessRepository.InsertAsync(new LeadWaitingProcessEntity()
                                        {
                                            BatchKey = "COIN_GECKO",
                                            UserId = user.Id,
                                            RecordId = coin.CoinId,
                                            Source = CrawlConsts.Signal.Source.COIN_GECKO
                                        });
                                    }
                                    break;
                                default:
                                    break;
                            }

                            item.Succeed = true;
                        }
                        catch (Exception ex)
                        {
                            item.Note = ex.ToString();
                        }

                        item.Ended = true;
                        await _coinGeckoCoinWaitingProcessRepository.UpdateAsync(item);
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

        public async Task<TwitterUserDto> GetTwitterUserAsync(string screenName, TwitterAccountEntity currentAcc)
        {
            if (screenName.IsEmpty())
            {
                return null;
            }

            TwitterUserDto user;
            try
            {
                user = await _twitterFollowingCrawlService.GetByUsernameAsync(screenName);
                return user;
            }
            catch
            {
                user = null;
            }

            if (currentAcc == null)
            {
                return user;
            }

            try
            {
                var response = await GetUserFromTwitterService(screenName, currentAcc.AccountId);
                var jsonContent = JObject.Parse(response);
                var data = jsonContent["data"];
                var userData = data["user"];

                if (userData == null)
                {
                    return null;
                }

                if (userData["result"] == null)
                {
                    return null;
                }

                if (userData["result"]["__typename"].ParseIfNotNull<string>() == "UserUnavailable")
                {
                    // User is suspended. Không lấy đc data trả về
                    return null;
                }

                if (userData["result"]["legacy"] == null)
                {
                    return null;
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

                return user;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> GetUserFromTwitterService(string screenName, string accountId)
        {
            Task delay(TimeSpan timeSpan)
            {
                return Task.Delay(timeSpan);
            }

            TwitterAPIUserGetUserResponse response = null;
            try
            {
                response = await _twitterAPIUserService.GetUserByScreenNameAsync(screenName, accountId);
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
                    return null;
                }
            }

            return response?.JsonContent;
        }
    }
}
