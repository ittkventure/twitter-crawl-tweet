using Medallion.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.AlphaQuest.Hangfire.BackgroundJobs;
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
    public class TwitterFollowingCrawlJobArg_v2
    {
        public string BatchKey { get; set; }

        public string TwitterAccountId { get; set; }
    }

    public class TwitterFollowingCrawlJob_v2 : AsyncBackgroundJob<TwitterFollowingCrawlJobArg_v2>, ITransientDependency
    {
        private const string LOG_PREFIX = "[TwitterFollowingCrawlJob_v2] ";
        const string SHOULD_STOP = "should_stop";

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<TwitterFollowingCrawlRelationEntity, long> _twitterFollowingCrawlRelationRepository;
        private readonly IRepository<TwitterFollowingCrawlQueueEntity, long> _twitterFollowingCrawlQueueRepository;
        private readonly IClock _clock;
        private readonly TwitterAPIUserService _twitterUserFollowingService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ITwitterAccountRepository _twitterAccountRepository;
        private readonly IDistributedLockProvider _distributedLockProvider;

        public TwitterFollowingCrawlJob_v2(
            IBackgroundJobManager backgroundJobManager,
            IRepository<TwitterFollowingCrawlRelationEntity, long> twitterFollowingCrawlRelationRepository,
            IRepository<TwitterFollowingCrawlQueueEntity, long> twitterFollowingCrawlQueueRepository,
            IClock clock,
            TwitterAPIUserService twitterUserFollowingService,
            IUnitOfWorkManager unitOfWorkManager,
            ITwitterAccountRepository twitterAccountRepository,
            IDistributedLockProvider distributedLockProvider)
        {
            _backgroundJobManager = backgroundJobManager;
            _twitterFollowingCrawlRelationRepository = twitterFollowingCrawlRelationRepository;
            _twitterFollowingCrawlQueueRepository = twitterFollowingCrawlQueueRepository;
            _clock = clock;
            _twitterUserFollowingService = twitterUserFollowingService;
            _unitOfWorkManager = unitOfWorkManager;
            _twitterAccountRepository = twitterAccountRepository;
            _distributedLockProvider = distributedLockProvider;
        }

        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(TwitterFollowingCrawlJobArg_v2 args)
        {
            using var uow = _unitOfWorkManager.Begin();
            try
            {
                await using (var handle = await _distributedLockProvider.TryAcquireLockAsync($"TwitterFollowingCrawlJob_{args.BatchKey}_{args.TwitterAccountId}"))
                {
                    if (handle == null)
                    {
                        Logger.LogInformation(LOG_PREFIX + "Some process is running!!!");
                        return;
                    }
                    var asyncExecuter = _twitterFollowingCrawlQueueRepository.AsyncExecuter;

                    var queueQuery = from input in await _twitterFollowingCrawlQueueRepository.GetQueryableAsync()
                                     where input.Ended == false && input.BatchKey == args.BatchKey && input.TwitterAccountId == args.TwitterAccountId
                                     select input;

                    var queue = await asyncExecuter.FirstOrDefaultAsync(queueQuery);
                    if (queue == null)
                    {
                        return;
                    }

                    var crawlAccount = await _twitterAccountRepository.FirstOrDefaultAsync(x => x.AccountId == args.TwitterAccountId);
                    if (crawlAccount == null)
                    {
                        throw new BusinessException(CrawlDomainErrorCodes.NotFound, $"Crawl Account {args.TwitterAccountId} not found");
                    }

                    var relationShips = new List<TwitterFollowingCrawlRelationEntity>();

                    bool succeed = false;
                    queue.ProcessedAttempt++;

                    try
                    {
                        var entries = new List<TwitterAPIEntryDto>();
                        var (succeedGetFollowing, messageGetFollowing, newCursor) = await GetFollowings(queue.UserId, crawlAccount.AccountId, entries, queue.CurrentCursor);

                        queue.CurrentCursor = newCursor;

                        if (!succeedGetFollowing)
                        {
                            queue.Note = messageGetFollowing;
                        }
                        else
                        {
                            var followings = entries.Where(x => x.EntryId.StartsWith("user"));
                            if (followings.IsEmpty())
                            {
                                if (queue.ProcessedAttempt == 1)
                                {
                                    queue.Note = "This User has not followed any accounts yet!";
                                }
                                
                                queue.Ended = true;
                            }
                            else
                            {
                                foreach (var data in followings)
                                {
                                    var userResult = data?.Content?.ItemContent?.UserResults?.Result;
                                    if (userResult == null || userResult.Typename == "UserUnavailable")
                                    {
                                        continue;
                                    }

                                    var relation = new TwitterFollowingCrawlRelationEntity()
                                    {
                                        BatchKey = args.BatchKey,
                                        UserId = queue.UserId,
                                        FollowingUserId = userResult.RestId,
                                        FollowingUserScreenName = userResult.Legacy?.ScreenName?.Replace("\0", string.Empty),
                                        FollowingUserName = userResult.Legacy?.Name?.Replace("\0", string.Empty),
                                        FollowingUserProfileImageUrl = userResult.Legacy?.ProfileImageUrlHttps,
                                        FollowingUserDescription = userResult.Legacy?.Description?.Replace("\0", string.Empty),
                                    };

                                    relationShips.Add(relation);

                                    string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
                                    if (DateTime.TryParseExact(
                                        userResult.Legacy.CreatedAt,
                                        format,
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None,
                                        out DateTime userCreatedAt))
                                    {
                                        relation.FollowingUserCreatedAt = userCreatedAt;
                                    }

                                    relation.FollowingUserFollowersCount = userResult.Legacy?.FollowersCount ?? 0;
                                    relation.FollowingUserFastFollowersCount = userResult.Legacy?.FastFollowersCount ?? 0;
                                    relation.FollowingUserNormalFollowersCount = userResult.Legacy?.NormalFollowersCount ?? 0;
                                    relation.FollowingUserStatusesCount = userResult.Legacy?.StatusesCount ?? 0;
                                    relation.FollowingUserFavouritesCount = userResult.Legacy?.FavouritesCount ?? 0;
                                    relation.FollowingUserFriendsCount = userResult.Legacy?.FriendsCount ?? 0;
                                    relation.FollowingUserListedCount = userResult.Legacy?.ListedCount ?? 0;
                                    relation.DiscoveredTime = _clock.Now;
                                }
                                succeed = true;
                            }
                        }

                        if (relationShips.IsNotEmpty())
                        {
                            await _twitterFollowingCrawlRelationRepository.InsertManyAsync(relationShips);
                        }

                        await uow.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        queue.ErrorProcessedAttempt++;
                        Logger.LogError(ex, LOG_PREFIX + "An error occurred while retrieving the following data from User " + queue.UserId);
                        queue.Note = ex.Message;
                        await uow.RollbackAsync();
                    }

                    if (queue.CurrentCursor.IsNotEmpty() && queue.CurrentCursor != SHOULD_STOP)
                    {
                        // tiếp tục cho phép scan lại queue này
                        succeed = false;
                    }
                    
                    queue.UpdateProcessStatus_v2(succeed);
                    
                    await _twitterFollowingCrawlQueueRepository.UpdateAsync(queue);

                    await uow.SaveChangesAsync();

                    if (queue.Ended)
                    {
                        await AddQueueCheckFollowers(args.BatchKey);
                    }
                }
            }
            catch (Exception ex)
            {
                await uow.RollbackAsync();
                Logger.LogError(ex, LOG_PREFIX + "An error occurred while crawling twitter data");
            }

            await uow.CompleteAsync();
                        
            await _backgroundJobManager.EnqueueAsync(args);
        }

        /// <summary>
        /// Thực hiện đưa các influencer vừa được crawl vào xếp hàng để thực hiện quá trình Add Followers
        /// </summary>
        /// <param name="batchKey"></param>
        /// <returns></returns>
        private Task AddQueueCheckFollowers(string batchKey)
        {
            return _backgroundJobManager.EnqueueAsync(new TwitterFollowingAddQueueCheckRelationJobArg()
            {
                BatchKey = batchKey
            });
        }

        private async Task<(bool, string, string)> GetFollowings(string userId, string accountId, List<TwitterAPIEntryDto> result, string cursor = null)
        {
            TwitterAPIUserGetFollowingResponse response = null;
            try
            {
                response = await _twitterUserFollowingService.GetFollowingAsync(userId, accountId, cursor: cursor);
                if (response.RateLimit > 0 || response.TooManyRequest)
                {
                    var subtract = response.RateLimitResetAt.Value.Subtract(_clock.Now);
                    if (response.RateLimitRemaining <= 1)
                    {
                        await Task.Delay(subtract);
                        response = await _twitterUserFollowingService.GetFollowingAsync(userId, accountId, cursor: cursor);
                    }
                }
            }
            catch (BusinessException ex)
            {
                if (ex.Code == CrawlDomainErrorCodes.TwitterAuthorizationError)
                {
                    // chỉ cho login lại 1 lần
                    response = await _twitterUserFollowingService.GetFollowingAsync(userId, accountId, requiredLogin: true, cursor: cursor);
                }
                else
                {
                    return (false, ex.Message, SHOULD_STOP);
                }
            }

            if (response?.Data?.User?.Result?.Typename == "UserUnavailable")
            {
                return (false, "User Unavailable", SHOULD_STOP);
            }

            var timelineAddEntries = response?.Data?.User?.Result?.Timeline?.Timeline?.Instructions?.FirstOrDefault(x => x.Type == "TimelineAddEntries");
            if (timelineAddEntries == null)
            {
                return (false, "Do not have Instructions type TimelineAddEntries", SHOULD_STOP);
            }

            var entries = timelineAddEntries.Entries;
            var users = entries.Where(x => x.EntryId.StartsWith("user"));
            if (users.IsEmpty())
            {
                return (true, "Succeeded", SHOULD_STOP);
            }

            result.AddRange(users);
            var timelineCusor = entries.FirstOrDefault(x => x.EntryId.StartsWith("cursor-bottom"));
            if (timelineCusor == null)
            {
                return (true, "Can not get cursor bottom", SHOULD_STOP);
            }

            cursor = timelineCusor.Content.Value;

            return (true, "Succeeded", cursor);
        }
    }
}
