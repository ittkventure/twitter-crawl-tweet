using Medallion.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Tweet;
using TK.Twitter.Crawl.Tweet.AirTable;
using TK.Twitter.Crawl.Tweet.MemoryLock;
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
        private readonly IRepository<AirTableNoMentionEntity, long> _airTableNoMentionRepository;
        private readonly IRepository<TwitterUserEntity, long> _twitterUserRepository;
        private readonly IRepository<TwitterTweetEntity, long> _twitterTweetRepository;
        private readonly IRepository<TwitterTweetHashTagEntity, long> _twitterTweetHashTagRepository;
        private readonly AirTableNoMentionManager _airTableNoMentionManager;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedLockProvider _distributedLockProvider;
        private readonly MemoryLockProvider _memoryLockProvider;

        public AirTableNoMentionProcessWaitingJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<AirTableNoMentionWaitingProcessEntity, long> airTableNoMentionWaitingProcessRepository,
            IRepository<AirTableNoMentionEntity, long> airTableNoMentionRepository,
            IRepository<TwitterUserEntity, long> twitterUserRepository,
            IRepository<TwitterTweetEntity, long> twitterTweetRepository,
            IRepository<TwitterTweetHashTagEntity, long> twitterTweetHashTagRepository,
            AirTableNoMentionManager airTableNoMentionManager,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedLockProvider distributedLockProvider,
            MemoryLockProvider memoryLockProvider)
        {
            _backgroundJobManager = backgroundJobManager;
            _airTableNoMentionWaitingProcessRepository = airTableNoMentionWaitingProcessRepository;
            _airTableNoMentionRepository = airTableNoMentionRepository;
            _twitterUserRepository = twitterUserRepository;
            _twitterTweetRepository = twitterTweetRepository;
            _twitterTweetHashTagRepository = twitterTweetHashTagRepository;
            _airTableNoMentionManager = airTableNoMentionManager;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedLockProvider = distributedLockProvider;
            _memoryLockProvider = memoryLockProvider;
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
                    var asyncExecuter = _airTableNoMentionWaitingProcessRepository.AsyncExecuter;

                    var queueQuery = from input in await _airTableNoMentionWaitingProcessRepository.GetQueryableAsync()
                                     where input.Ended == false && input.Action == "PUSH"
                                     select input;

                    // xử lý theo chiều oldest -> lastest
                    queueQuery = queueQuery.OrderBy(x => x.CreationTime).Take(BATCH_SIZE);

                    var queues = await asyncExecuter.ToListAsync(queueQuery);
                    if (queues.IsEmpty())
                    {
                        return;
                    }

                    var tweets = await asyncExecuter.ToListAsync(

                        from t in await _twitterTweetRepository.GetQueryableAsync()
                        where queues.Select(x => x.RefId).Contains(t.TweetId)
                        select t
                        );

                    var tweetHashTags = await asyncExecuter.ToListAsync(

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
                            if (item.Action == "PUSH")
                            {
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
                            }

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

    }
}
