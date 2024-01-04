using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Notification;
using TK.Twitter.Crawl.TwitterAPI;
using TK.Twitter.Crawl.TwitterAPI.Dto;
using TK.TwitterAccount.Domain;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.TenantManagement;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.Jobs
{
    public class TwitterTweetCrawlJobArg
    {
        public string BatchKey { get; set; }

        public string TwitterAccountId { get; set; }
    }

    public class TwitterTweetCrawlJob : AsyncBackgroundJob<TwitterTweetCrawlJobArg>, ITransientDependency
    {
        private const string LOG_PREFIX = "[TwitterTweetCrawlJob] ";
        public const int BATCH_SIZE = 1;

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<TwitterTweetCrawlQueueEntity, long> _twitterTweetCrawlQueueRepository;
        private readonly IRepository<TwitterTweetCrawlRawEntity, long> _twitterTweetCrawlRawRepository;
        private readonly IRepository<TwitterTweetEntity, long> _twitterTweetRepository;
        private readonly IRepository<TwitterTweetMediaEntity, long> _twitterTweetMediaRepository;
        private readonly IRepository<TwitterTweetUserMentionEntity, long> _twitterTweetUserMentionRepository;
        private readonly IRepository<TwitterTweetHashTagEntity, long> _twitterTweetHashTagRepository;
        private readonly IRepository<TwitterTweetUrlEntity, long> _twitterTweetUrlRepository;
        private readonly IRepository<TwitterTweetSymbolEntity, long> _twitterTweetSymbolRepository;
        private readonly IRepository<TwitterUserSignalEntity, long> _twitterUserSignalRepository;
        private readonly IRepository<TwitterUserTypeEntity, long> _twitterUserTypeRepository;
        private readonly IRepository<TwitterUserStatusEntity, long> _twitterUserStatusRepository;
        private readonly IRepository<LeadWaitingProcessEntity, long> _leadWaitingProcessEntityRepository;
        private readonly IRepository<AirTableNoMentionWaitingProcessEntity, long> _airTableNoMentionWaitingProcessRepository;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly TwitterAPITweetService _twitterAPITweetService;
        private readonly ITwitterAccountRepository _twitterAccountRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedEventBus _distributedEventBus;

        public TwitterTweetCrawlJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<TwitterTweetCrawlQueueEntity, long> twitterTweetCrawlQueueRepository,
            IRepository<TwitterTweetCrawlRawEntity, long> twitterTweetCrawlRawRepository,
            IRepository<TwitterTweetEntity, long> twitterTweetRepository,
            IRepository<TwitterTweetMediaEntity, long> twitterTweetMediaRepository,
            IRepository<TwitterTweetUserMentionEntity, long> twitterTweetUserMentionRepository,
            IRepository<TwitterTweetHashTagEntity, long> twitterTweetHashTagRepository,
            IRepository<TwitterTweetUrlEntity, long> twitterTweetUrlRepository,
            IRepository<TwitterTweetSymbolEntity, long> twitterTweetSymbolRepository,
            IRepository<TwitterUserSignalEntity, long> twitterUserSignalRepository,
            IRepository<TwitterUserTypeEntity, long> twitterUserTypeRepository,
            IRepository<TwitterUserStatusEntity, long> twitterUserStatusRepository,
            IRepository<LeadWaitingProcessEntity, long> leadWaitingProcessEntityRepository,
            IRepository<AirTableNoMentionWaitingProcessEntity, long> airTableNoMentionWaitingProcessRepository,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            TwitterAPITweetService twitterAPITweetService,
            ITwitterAccountRepository twitterAccountRepository,
            IMemoryCache memoryCache,
            IDistributedEventBus distributedEventBus)
        {
            _backgroundJobManager = backgroundJobManager;
            _twitterTweetCrawlQueueRepository = twitterTweetCrawlQueueRepository;
            _twitterTweetCrawlRawRepository = twitterTweetCrawlRawRepository;
            _twitterTweetRepository = twitterTweetRepository;
            _twitterTweetMediaRepository = twitterTweetMediaRepository;
            _twitterTweetUserMentionRepository = twitterTweetUserMentionRepository;
            _twitterTweetHashTagRepository = twitterTweetHashTagRepository;
            _twitterTweetUrlRepository = twitterTweetUrlRepository;
            _twitterTweetSymbolRepository = twitterTweetSymbolRepository;
            _twitterUserSignalRepository = twitterUserSignalRepository;
            _twitterUserTypeRepository = twitterUserTypeRepository;
            _twitterUserStatusRepository = twitterUserStatusRepository;
            _leadWaitingProcessEntityRepository = leadWaitingProcessEntityRepository;
            _airTableNoMentionWaitingProcessRepository = airTableNoMentionWaitingProcessRepository;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _twitterAPITweetService = twitterAPITweetService;
            _twitterAccountRepository = twitterAccountRepository;
            _memoryCache = memoryCache;
            _distributedEventBus = distributedEventBus;
        }

        #region Jobs


        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(TwitterTweetCrawlJobArg args)
        {
            var cacheKey = $"TwitterTweetCrawlJob_{args.BatchKey}_{args.TwitterAccountId}";
            // Kiểm tra xem task đã được thực hiện bởi người dùng khác chưa
            if (IsTaskInProgress(cacheKey))
            {
                return;
            }

            // Thực hiện task và đặt trạng thái task is-in-progress vào cache
            SetTaskInProgress(cacheKey);

            try
            {
                using var uow = _unitOfWorkManager.Begin();
                try
                {
                    var asyncExecuter = _twitterTweetCrawlQueueRepository.AsyncExecuter;

                    var queueQuery = from input in await _twitterTweetCrawlQueueRepository.GetQueryableAsync()
                                     where input.Ended == false && input.BatchKey == args.BatchKey && input.TwitterAccountId == args.TwitterAccountId
                                     select input;

                    queueQuery = queueQuery.Take(BATCH_SIZE);

                    var queues = await asyncExecuter.ToListAsync(queueQuery);
                    if (queues.IsEmpty())
                    {
                        ClearTaskInProgress(cacheKey);
                        return;
                    }

                    var crawlAccount = await _twitterAccountRepository.FirstOrDefaultAsync(x => x.AccountId == args.TwitterAccountId);
                    if (crawlAccount == null)
                    {
                        throw new BusinessException(CrawlDomainErrorCodes.NotFound, $"Crawl Account {args.TwitterAccountId} not found");
                    }

                    foreach (var item in queues)
                    {
                        bool succeeded = false;
                        item.ProcessedAttempt++;

                        try
                        {
                            var entries = new List<TwitterTweetCrawlRawEntity>();
                            string messageGet = string.Empty;
                            bool succeedGet = false;
                            bool shouldStop = false;
                            string cursor = null;
                            string accountId = args.TwitterAccountId;
                            int loop = 0;

                            while (true)
                            {
                                loop++;
                                if (loop == 3)
                                {
                                    succeedGet = true;
                                    messageGet = "Succeed";
                                    break;
                                }

                                TwitterAPIGetTweetResponse response = null;
                                try
                                {
                                    response = await _twitterAPITweetService.GetTweetAsync(item.UserId, accountId, cursor: cursor);
                                    if (response.RateLimit > 0 || response.TooManyRequest)
                                    {
                                        var subtract = response.RateLimitResetAt.Value.Subtract(_clock.Now);
                                        if (response.RateLimitRemaining <= 1)
                                        {
                                            Logger.LogInformation(LOG_PREFIX + "Delay in " + subtract);
                                            await Task.Delay(subtract);
                                            response = await _twitterAPITweetService.GetTweetAsync(item.UserId, accountId, cursor: cursor);
                                        }
                                    }
                                }
                                catch (BusinessException ex)
                                {
                                    if (ex.Code == CrawlDomainErrorCodes.TwitterAuthorizationError)
                                    {
                                        // chỉ cho login lại 1 lần
                                        response = await _twitterAPITweetService.GetTweetAsync(item.UserId, accountId, cursor: cursor);
                                    }
                                    else
                                    {
                                        succeedGet = false;
                                        messageGet = ex.Message;
                                        break;
                                    }
                                }

                                var jsonContent = JObject.Parse(response.JsonText);
                                if (jsonContent["data"]?["user"]?["result"] == null)
                                {
                                    succeedGet = false;
                                    messageGet = "User Unavailable";
                                    break;
                                }

                                if ((string)jsonContent["data"]["user"]["result"]["__typename"] == "UserUnavailable")
                                {
                                    succeedGet = false;
                                    messageGet = "User Unavailable";
                                    break;
                                }

                                var timelineAddEntryQuery = from jo in jsonContent["data"]["user"]["result"]["timeline_v2"]["timeline"]["instructions"]
                                                            where jo["type"].Value<string>() == "TimelineAddEntries"
                                                            select jo;

                                var timelineAddEntries = timelineAddEntryQuery.FirstOrDefault();
                                if (timelineAddEntries == null)
                                {
                                    succeedGet = false;
                                    messageGet = "Do not have Instructions type TimelineAddEntries";
                                    break;
                                }

                                var tweetInPages = new List<TwitterTweetCrawlRawEntity>();
                                foreach (var entry in timelineAddEntries["entries"])
                                {
                                    if (entry["entryId"].Value<string>().StartsWith("cursor-bottom"))
                                    {
                                        cursor = entry["content"]["value"].Value<string>();
                                        continue;
                                    }

                                    if (!entry["entryId"].Value<string>().StartsWith("tweet"))
                                    {
                                        continue;
                                    }

                                    var content = entry["content"];
                                    if (content["entryType"].Value<string>() != "TimelineTimelineItem")
                                    {
                                        continue;
                                    }

                                    var itemContent = content["itemContent"];
                                    if (itemContent["itemType"].Value<string>() != "TimelineTweet")
                                    {
                                        continue;
                                    }

                                    var result_type = itemContent["tweet_results"]["result"]["__typename"].ParseIfNotNull<string>();
                                    var tweetResult = itemContent["tweet_results"]["result"];

                                    string tweetId;
                                    if (result_type == "TweetWithVisibilityResults")
                                    {
                                        tweetId = tweetResult["tweet"]["rest_id"].ParseIfNotNull<string>();
                                    }
                                    else
                                    {
                                        tweetId = tweetResult["rest_id"].ParseIfNotNull<string>();
                                    }

                                    var createdAt = GetTweetCreatedAt(entry);
                                    if (createdAt.HasValue && createdAt.Value < new DateTime(2023, 6, 1))
                                    {
                                        shouldStop = true;
                                        break;
                                    }

                                    if (tweetInPages.Any(x => x.TweetId == tweetId))
                                    {
                                        continue;
                                    }

                                    tweetInPages.Add(new TwitterTweetCrawlRawEntity()
                                    {
                                        TweetId = tweetId,
                                        JsonContent = JsonHelper.Stringify(entry)
                                    });
                                }

                                if (tweetInPages.IsEmpty())
                                {
                                    succeedGet = true;
                                    messageGet = "Succeed";
                                    break;
                                }

                                entries.AddRange(tweetInPages);

                                if (shouldStop)
                                {
                                    succeedGet = true;
                                    messageGet = "Succeed";
                                    break;
                                }
                            }

                            if (!succeedGet)
                            {
                                item.Note = messageGet;
                            }
                            else
                            {
                                if (entries.IsEmpty())
                                {
                                    item.Note = "This User has not any tweets yet!";
                                    item.Ended = true;
                                }
                                else
                                {
                                    var tweetIds = entries.Select(x => x.TweetId);
                                    var tweetsHasExisted = await _twitterTweetCrawlRawRepository.GetListAsync(x => tweetIds.Contains(x.TweetId));
                                    var addedTweetIds = new List<string>();
                                    int count = 0;
                                    foreach (var data in entries)
                                    {
                                        count++;
                                        if (tweetsHasExisted.Any(t => t.TweetId == data.TweetId))
                                        {
                                            continue;
                                        }

                                        if (addedTweetIds.Contains(data.TweetId))
                                        {
                                            continue;
                                        }

                                        await _twitterTweetCrawlRawRepository.InsertAsync(new TwitterTweetCrawlRawEntity
                                        {
                                            TweetId = data.TweetId,
                                            JsonContent = data.JsonContent
                                        });

                                        await AddTweet(item.UserId, data.TweetId, data.JsonContent, item.Tags, item.BatchKey);

                                        addedTweetIds.Add(data.TweetId);
                                    }

                                    succeeded = true;
                                }
                            }

                            await uow.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, LOG_PREFIX + "An error occurred while retrieving the following data from User " + item.UserId);
                            item.Note = ex.Message;
                            await uow.RollbackAsync();
                        }

                        item.UpdateProcessStatus(succeeded);

                        await _twitterTweetCrawlQueueRepository.UpdateAsync(item);
                        await uow.SaveChangesAsync();
                    }

                    await uow.SaveChangesAsync();
                    await uow.CompleteAsync();
                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync();
                    Logger.LogError(ex, LOG_PREFIX + "An error occurred while crawling twitter data");
                }

                // Sau khi hoàn thành task, xóa trạng thái task is-in-progress từ cache
                ClearTaskInProgress(cacheKey);

                await _backgroundJobManager.EnqueueAsync(args);
            }
            catch (Exception ex)
            {
                await _distributedEventBus.PublishAsync(new NotificationErrorEto()
                {
                    Tags = "[TwitterTweetCrawlJob]",
                    Message = "Lỗi khi crawl tweet. Detail: " + ex.Message,
                    ExceptionStackTrace = ex.StackTrace
                });

                ClearTaskInProgress(cacheKey);
            }
        }

        private bool IsTaskInProgress(string cacheKey)
        {
            return _memoryCache.TryGetValue(cacheKey, out bool isTaskInProgress) && isTaskInProgress;
        }

        private void SetTaskInProgress(string cacheKey)
        {
            _memoryCache.Set(cacheKey, true);
        }

        private void ClearTaskInProgress(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);
        }

        #endregion

        #region Add Tweet

        public async Task AddTweet(string mediaMentionedUserId, string tweetId, string jsonContent, string mediaMentionedTags, string batchKey)
        {
            var entry = JObject.Parse(jsonContent);
            var content = entry["content"];
            var itemContent = content["itemContent"];
            var tweetResult = itemContent["tweet_results"]["result"];
            var result_type = itemContent["tweet_results"]["result"]["__typename"].ParseIfNotNull<string>();
            if (result_type == "TweetWithVisibilityResults")
            {
                tweetResult = tweetResult["tweet"];
            }

            var tweet = new TwitterTweetEntity()
            {
                UserId = mediaMentionedUserId,
                TweetId = tweetId
            };

            if (tweetResult["views"]?["count"] != null)
            {
                tweet.ViewsCount = tweetResult["views"]["count"].Value<int>();
            }

            var tweetUser = tweetResult["core"]?["user_results"]?["result"]?["legacy"];
            if (tweetUser != null)
            {
                tweet.UserName = tweetUser["name"].ParseIfNotNull<string>();
                tweet.UserScreenName = tweetUser["screen_name"].ParseIfNotNull<string>();
                tweet.UserScreenNameNormalize = tweet.UserScreenName?.ToLower();
            }

            var tweetLegacy = tweetResult["legacy"];

            string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            if (DateTime.TryParseExact(
                tweetLegacy["created_at"].Value<string>(),
                format,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime createdAt))
            {
                tweet.CreatedAt = createdAt;
            }

            tweet.BookmarkCount = tweetLegacy["bookmark_count"].Value<int>();
            tweet.FavoriteCount = tweetLegacy["favorite_count"].Value<int>();
            tweet.QuoteCount = tweetLegacy["quote_count"].Value<int>();
            tweet.ReplyCount = tweetLegacy["reply_count"].Value<int>();
            tweet.RetweetCount = tweetLegacy["retweet_count"].Value<int>();

            tweet.IsQuoteStatus = tweetLegacy["is_quote_status"].Value<bool>();

            var retweetdStatusResult = tweetLegacy["retweeted_status_result"];
            if (retweetdStatusResult != null)
            {
                if (retweetdStatusResult["result"]?["__typename"].ParseIfNotNull<string>() == "TweetWithVisibilityResults")
                {
                    tweet.FullText = retweetdStatusResult["result"]?["tweet"]?["legacy"]?["full_text"].Value<string>();
                }
                else
                {
                    tweet.FullText = retweetdStatusResult["result"]?["legacy"]?["full_text"].Value<string>();
                }
            }
            else
            {
                tweet.FullText = tweetLegacy["full_text"].Value<string>();
            }

            tweet.NormalizeFullText = tweet.FullText?.ToLower();

            tweet.Lang = tweetLegacy["lang"].Value<string>();

            if (tweetLegacy["in_reply_to_screen_name"] != null)
            {
                tweet.InReplyToScreenName = tweetLegacy["in_reply_to_screen_name"].Value<string>();
            }

            if (tweetLegacy["in_reply_to_status_id_str"] != null)
            {
                tweet.InReplyToStatusId = tweetLegacy["in_reply_to_status_id_str"].Value<string>();
            }

            if (tweetLegacy["in_reply_to_user_id_str"] != null)
            {
                tweet.InReplyToUserId = tweetLegacy["in_reply_to_user_id_str"].Value<string>();
            }

            if (tweetLegacy["conversation_id_str"] != null)
            {
                tweet.ConversationId = tweetLegacy["conversation_id_str"].Value<string>();
            }

            await _twitterTweetRepository.InsertAsync(tweet);

            if (tweetLegacy["entities"] != null)
            {
                if (tweetLegacy["entities"]["media"] != null)
                {
                    foreach (var item in tweetLegacy["entities"]["media"])
                    {
                        var id_str = item["id_str"].ParseIfNotNull<string>();
                        var type = item["type"].ParseIfNotNull<string>();
                        var expanded_url = item["expanded_url"].ParseIfNotNull<string>();
                        var display_url = item["display_url"].ParseIfNotNull<string>();
                        var url = item["url"].ParseIfNotNull<string>();
                        var media_url_https = item["media_url_https"].ParseIfNotNull<string>();

                        await _twitterTweetMediaRepository.InsertAsync(new TwitterTweetMediaEntity()
                        {
                            TweetId = tweetId,
                            MediaId = id_str,
                            Type = type,
                            Url = url,
                            DisplayUrl = display_url,
                            ExpandedUrl = expanded_url,
                            MediaUrlHttps = media_url_https,
                        });
                    }
                }

                var mentions = new List<TwitterTweetUserMentionEntity>();
                if (tweetLegacy["entities"]["user_mentions"] != null)
                {
                    foreach (var item in tweetLegacy["entities"]["user_mentions"])
                    {
                        var id_str = item["id_str"].ParseIfNotNull<string>();
                        var name = item["name"].ParseIfNotNull<string>();
                        var screen_name = item["screen_name"].ParseIfNotNull<string>();

                        if (mentions.Any(x => x.UserId == id_str))
                        {
                            continue;
                        }

                        mentions.Add(new TwitterTweetUserMentionEntity()
                        {
                            TweetId = tweetId,
                            UserId = id_str,
                            Name = name,
                            ScreenName = screen_name,
                            NormalizeName = name.ToLower(),
                            NormalizeScreenName = screen_name.ToLower(),
                            TweetCreatedAt = tweet.CreatedAt
                        });

                    }
                }

                if (tweetLegacy["entities"]["symbols"] != null)
                {
                    foreach (var item in tweetLegacy["entities"]["symbols"])
                    {
                        var text = item["text"].ParseIfNotNull<string>();
                        await _twitterTweetSymbolRepository.InsertAsync(new TwitterTweetSymbolEntity()
                        {
                            TweetId = tweetId,
                            SymbolText = text,
                        });
                    }
                }

                var tags = new List<TwitterTweetHashTagEntity>();
                if (tweetLegacy["entities"]["hashtags"] != null)
                {
                    foreach (var item in tweetLegacy["entities"]["hashtags"])
                    {
                        var text = item["text"].ParseIfNotNull<string>();
                        tags.Add(new TwitterTweetHashTagEntity()
                        {
                            TweetId = tweetId,
                            Text = text,
                            NormalizeText = text.ToLower(),
                        });

                    }
                }

                if (tweetLegacy["entities"]["urls"] != null)
                {
                    foreach (var item in tweetLegacy["entities"]["urls"])
                    {
                        var display_url = item["display_url"].ParseIfNotNull<string>();
                        var expanded_url = item["expanded_url"].ParseIfNotNull<string>();
                        var url = item["url"].ParseIfNotNull<string>();

                        await _twitterTweetUrlRepository.InsertAsync(new TwitterTweetUrlEntity()
                        {
                            TweetId = tweetId,
                            ExpandedUrl = expanded_url,
                            DisplayUrl = display_url,
                            Url = url
                        });

                    }
                }

                if (tags.IsNotEmpty())
                {
                    await _twitterTweetHashTagRepository.InsertManyAsync(tags);
                }

                // Check Signals
                if (mentions.IsNotEmpty())
                {
                    await _twitterTweetUserMentionRepository.InsertManyAsync(mentions);

                    await ProcessSignalWithUserMention(new()
                    {
                        BatchKey = batchKey,
                        MediaMentionedTags = mediaMentionedTags,
                        MediaMentionedUserId = mediaMentionedUserId,
                        Mentions = mentions,
                        Tweet = tweet,
                        Tags = tags
                    });
                }
                else
                {
                    // Check No mention
                    var signals = GetSignals(mediaMentionedUserId, tweet.NormalizeFullText, mediaMentionedTags, tags.Select(x => x.NormalizeText));
                    if (signals.IsNotEmpty())
                    {
                        await _airTableNoMentionWaitingProcessRepository.InsertAsync(new AirTableNoMentionWaitingProcessEntity()
                        {
                            RefId = tweet.TweetId,
                            Signals = signals.JoinAsString(","),
                            Action = "PUSH",
                        });
                    }
                }
            }
        }

        private static DateTime? GetTweetCreatedAt(JToken entry)
        {
            var content = entry["content"];
            var itemContent = content["itemContent"];
            var tweetResult = itemContent["tweet_results"]["result"];
            var result_type = itemContent["tweet_results"]["result"]["__typename"].ParseIfNotNull<string>();
            if (result_type == "TweetWithVisibilityResults")
            {
                tweetResult = tweetResult["tweet"];
            }

            var tweetLegacy = tweetResult["legacy"];

            string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            if (DateTime.TryParseExact(
                tweetLegacy["created_at"].Value<string>(),
                format,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime createdAt))
            {
                return createdAt;
            }

            return null;
        }

        #endregion


        #region Process Signals

        public class ProcessSignalWithUserMentionContext
        {
            public TwitterTweetEntity Tweet { get; set; }

            public List<TwitterTweetUserMentionEntity> Mentions { get; set; }

            public List<TwitterTweetHashTagEntity> Tags { get; set; }

            public string MediaMentionedUserId { get; set; }

            public string MediaMentionedTags { get; set; }

            public string BatchKey { get; set; }
        }

        public async Task ProcessSignalWithUserMention(ProcessSignalWithUserMentionContext context)
        {
            var mentions = context.Mentions;

            if (context.MediaMentionedTags.IsNotEmpty())
            {
                if (context.MediaMentionedTags.Contains("just_raised_funds"))
                {
                    if (context.MediaMentionedUserId == "1635678642499100672") // screen_name: CryptoRank_VCs      name: Fundraising Digest
                    {
                        if (context.Tweet.FullText.StartsWith("⚡️"))
                        {
                            // project sẽ được mention đầu tiên và các mention sau sẽ là các ventures nên chỉ cần quan tâm đến mention đầu tiên
                            mentions = new List<TwitterTweetUserMentionEntity> { mentions[0] };
                        }
                        else
                        {
                            // Các tweet khác không phải template này thì k cần check gì thêm
                            return;
                        }
                    }
                    else if (context.MediaMentionedUserId == "1222812013002444800") // screen_name: Crypto_Dealflow      name: Crypto Fundraising
                    {
                        // project sẽ được mention đầu tiên và các mention sau sẽ là các ventures nên chỉ cần quan tâm đến mention đầu tiên
                        mentions = new List<TwitterTweetUserMentionEntity> { mentions[0] };
                    }
                }

                if (context.MediaMentionedTags.Contains("building_on_bnb_chain"))
                {
                    if (context.MediaMentionedUserId == "1052454006537314306") // screen_name: BNBCHAIN     name: BNB Chain
                    {
                        if (!context.Tweet.FullText.ToLower().Contains("welcome"))
                        {
                            // Các tweet khác không phải template này thì k cần check gì thêm
                            return;
                        }
                    }
                }
            }

            foreach (var item in mentions)
            {
                if (item.UserId == context.Tweet.UserId) // bỏ qua mention chính nó
                {
                    continue;
                }

                if (item.UserId == "-1") // bỏ qua các mention đến user suspended
                {
                    continue;
                }

                if (item.NormalizeScreenName.In(
                    "binance",
                    "coinbase",
                    "bnbchain",
                    "epicgames",
                    "bitfinex",
                    "bitmartexchange"
                )) // bỏ qua các mention đến user lớn để bú fame
                {
                    continue;
                }

                // loại bỏ thằng này vì tự tag mình vào các bài listing cmc/cgk
                if (item.UserId == CrawlConsts.TwitterUser.BOT_OWNER_NEW_LISTING_CMC_CGK_USER_ID)
                {
                    continue;
                }

                var signals = GetSignals(
                    context.MediaMentionedUserId, 
                    context.Tweet.NormalizeFullText, 
                    context.MediaMentionedTags, 
                    context.Tags.Select(x => x.NormalizeText));

                await ProcessSignals(signals, item.UserId, item.TweetId, context.BatchKey);
            }
        }

        public async Task ProcessSignals(IEnumerable<string> signals, string mentionedUserId, string tweetId, string batchKey)
        {
            if (signals.IsEmpty())
            {
                return;
            }

            bool shouldAddWaitingProcess = false;

            // Thêm signal cho lead
            foreach (var signal in signals)
            {
                bool shouldAddSignal = true;
                if (signal == CrawlConsts.Signal.JUST_RAISED_FUNDS)
                {
                    // Trong trường hợp này đang crawl từ 2 nguồn và có thể bị trùng nhau. Nếu Project đã có signal này thì bỏ qua k cần add thêm
                    shouldAddSignal = !await _twitterUserSignalRepository.AnyAsync(x => x.UserId == mentionedUserId && x.Signal == CrawlConsts.Signal.JUST_RAISED_FUNDS);
                }

                if (shouldAddSignal)
                {
                    shouldAddWaitingProcess = true;
                    await _twitterUserSignalRepository.InsertAsync(new TwitterUserSignalEntity()
                    {
                        UserId = mentionedUserId,
                        TweetId = tweetId,
                        Signal = signal,
                        Source = CrawlConsts.Signal.Source.TWITTER_TWEET
                    });
                }
            }

            if (shouldAddWaitingProcess)
            {
                // Đưa vào waiting process
                await _leadWaitingProcessEntityRepository.InsertAsync(new LeadWaitingProcessEntity()
                {
                    BatchKey = batchKey,
                    UserId = mentionedUserId,
                    TweetId = tweetId,
                    Source = CrawlConsts.Signal.Source.TWITTER_TWEET
                });

                // Nếu Signal thỏa mã điều kiện thì đưa Lead thành Type Lead luôn
                if (LeadProcessWaitingJob.IsLeadBySignalCode(signals))
                {
                    var userType = await _twitterUserTypeRepository.FirstOrDefaultAsync(x => x.UserId == mentionedUserId);
                    if (userType == null)
                    {
                        await _twitterUserTypeRepository.InsertAsync(new TwitterUserTypeEntity()
                        {
                            UserId = mentionedUserId,
                            Type = CrawlConsts.LeadType.LEADS,
                            IsUserSuppliedValue = false,
                        }, autoSave: true);
                    }
                    else
                    {
                        if (!userType.IsUserSuppliedValue && userType.Type != CrawlConsts.LeadType.LEADS)
                        {
                            userType.Type = CrawlConsts.LeadType.LEADS;
                            await _twitterUserTypeRepository.UpdateAsync(userType);
                        }
                    }
                }

                var userStatus = await _twitterUserStatusRepository.FirstOrDefaultAsync(x => x.UserId == mentionedUserId);
                if (userStatus == null)
                {
                    await _twitterUserStatusRepository.InsertAsync(new TwitterUserStatusEntity()
                    {
                        UserId = mentionedUserId,
                        Status = "New",
                        IsUserSuppliedValue = false,
                    }, autoSave: true);
                }
                else
                {
                    if (!userStatus.IsUserSuppliedValue && userStatus.Status != "New")
                    {
                        userStatus.Status = "New";
                        await _twitterUserStatusRepository.UpdateAsync(userStatus);
                    }
                }
            }
        }

        public static IEnumerable<string> GetSignals(string mediaMentionedUserId, string tweetFullText, string mediaMentionedTags, IEnumerable<string> tweetTags)
        {
            tweetFullText ??= string.Empty;

            List<string> signals = new();
            if (mediaMentionedTags.IsNotEmpty())
            {
                if (mediaMentionedTags.Contains("cex"))
                {
                    bool check;
                    switch (mediaMentionedUserId)
                    {
                        case "978566222282444800": // MEXC_Official
                            check = tweetFullText.Contains("deposit") && tweetFullText.Contains("trading");
                            break;
                        case "912539725071777792": // gate_io
                            check = tweetFullText.Contains("listing") && tweetFullText.Contains("trading");
                            break;
                        case "1098881129057112064": // bitgetglobal
                        case "999947328621395968": // Bybit_Official
                        case "937166242208763904": // BitMartExchange
                            check = tweetFullText.Contains("listing") && tweetFullText.Contains("deposit");
                            break;
                        case "910110294625492992": // kucoincom
                            check = tweetFullText.Contains("listing") && tweetFullText.Contains("new");
                            break;
                        case "957221394009354245": // bitforexcom
                            check = tweetTags.Contains("newlistings");
                            break;
                        default:
                            check = false;
                            break;
                    }

                    if (check)
                    {
                        signals.Add(CrawlConsts.Signal.LISTING_CEX);
                    }
                }

                if (mediaMentionedTags.Contains("audit"))
                {
                    bool check;
                    switch (mediaMentionedUserId)
                    {
                        case "993673575230996480": // certik
                        case "1571404564540047361": // securewiseAudit
                        case "1390840374": // TechRightio
                            check = tweetFullText.Contains("complete") && tweetFullText.Contains("audit");
                            break;
                        case "1478527011140214784": // contractwolf_io
                            check = tweetFullText.Contains("audit") && tweetFullText.Contains("report");
                            break;
                        case "1370160171822018560": // AssureDefi
                            check = tweetFullText.Contains("assured");
                            break;
                        case "1496534305891311622": // CoinsultAudits
                        case "1461394993537589248": // VB_Audit
                        case "1398253738905714690": // SolidProof_io
                            check = tweetFullText.Contains("audit") && tweetFullText.Contains("complete");
                            break;
                        case "898528774303735808": // hackenclub
                            check = tweetFullText.Contains("audit") && (tweetFullText.Contains("complete") || tweetFullText.Contains("successfully"));
                            break;
                        default:
                            check = false;
                            break;
                    }

                    if (check)
                    {
                        signals.Add(CrawlConsts.Signal.JUST_AUDITED);
                    }
                }

                if (mediaMentionedTags.Contains("upcoming_token_sale"))
                {
                    bool check;
                    switch (mediaMentionedUserId)
                    {
                        case "1402594955047043074": // Spores_Network
                            check = (tweetFullText.Contains("ido") && tweetFullText.Contains("launching")) || (tweetTags.Contains("ido") && tweetTags.Contains("launching"));
                            break;
                        case "1171766610031341568": // Kingdomstarter
                            check = (tweetFullText.Contains("ido") && tweetFullText.Contains("commit")) || (tweetTags.Contains("ido") && tweetTags.Contains("commit"));
                            break;
                        case "1384903770392387586": // bullperks
                            check = (tweetFullText.Contains("ido") && tweetFullText.Contains("upcoming")) || (tweetTags.Contains("ido") && tweetTags.Contains("upcoming"));
                            break;
                        case "1277204360662077440": // trustswap
                            check = tweetFullText.Contains("new") && tweetFullText.Contains("launchpad") || (tweetTags.Contains("new") && tweetTags.Contains("launchpad"));

                            if (!check)
                            {
                                check = (tweetFullText.Contains("next") && tweetFullText.Contains("launchpad")) || (tweetTags.Contains("next") && tweetTags.Contains("launchpad"));
                            }

                            if (!check)
                            {
                                check = (tweetFullText.Contains("upcoming") && tweetFullText.Contains("launchpad")) || (tweetTags.Contains("upcoming") && tweetTags.Contains("launchpad"));
                            }

                            if (!check)
                            {
                                check = (tweetFullText.Contains("incoming") && tweetFullText.Contains("launchpad")) || (tweetTags.Contains("incoming") && tweetTags.Contains("launchpad"));
                            }

                            if (!check)
                            {
                                check = (tweetFullText.Contains("introducing") && tweetFullText.Contains("launchpad")) || (tweetTags.Contains("introducing") && tweetTags.Contains("launchpad"));
                            }
                            break;
                        case "1514973302280048647": // Gateio_Startup
                            check = (tweetFullText.Contains("launchpad")) || (tweetTags.Contains("launchpad"));
                            break;
                        case "957221394009354245": // bitforexcom
                            check = (tweetFullText.Contains("ieo")) || (tweetTags.Contains("ieo"));
                            break;
                        case "937166242208763904": // BitMartExchange
                            check = (tweetFullText.Contains("launchpad")) || (tweetTags.Contains("launchpad"));
                            break;
                        case "1389870631735414787": // kommunitas1
                            check = (tweetFullText.Contains("iko")) || (tweetTags.Contains("iko"));
                            break;
                        case "999947328621395968": // Bybit_Official
                            check = ((tweetFullText.Contains("new") && tweetFullText.Contains("launchpad")) && tweetFullText.Contains("project")) || (tweetTags.Contains("launchpad") && tweetTags.Contains("project") && tweetTags.Contains("project"));
                            break;
                        case "1385154488449658880": // pinkecosystem
                            check = (tweetFullText.Contains("launchpad")) || (tweetTags.Contains("launchpad"));
                            break;
                        case "1300122153317212161": // Poolz__
                            check = (tweetFullText.Contains("ido") && tweetFullText.Contains("launching")) || (tweetTags.Contains("ido") && tweetTags.Contains("launching"));
                            break;
                        case "1460254342070550530": // thegempad
                            check = (tweetFullText.Contains("announcement") && tweetFullText.Contains("launch")) || (tweetTags.Contains("announcement") && tweetTags.Contains("launch"));
                            break;
                        case "1415522287126671363": // GameFi_Official
                            check = tweetFullText.Contains("whitelist") || (tweetTags.Contains("whitelist"));
                            break;
                        case "1368509175769092096": // BSClaunchorg
                            check = (tweetFullText.Contains("upcoming") && tweetFullText.Contains("ido")) || (tweetTags.Contains("upcoming") && tweetTags.Contains("ido"));
                            if (!check)
                            {
                                check = (tweetFullText.Contains("launch") && tweetFullText.Contains("ido")) || (tweetTags.Contains("launch") && tweetTags.Contains("ido"));
                            }
                            break;
                        case "1421126750214385672": // finblox
                            check = (tweetFullText.Contains("finlaunch") && tweetFullText.Contains("date")) || (tweetTags.Contains("finlaunch") && tweetTags.Contains("date"));
                            break;
                        case "1345102860732788740": // SeedifyFund                            
                            check = tweetFullText.Contains("launch") && tweetFullText.Contains("ido") || (tweetTags.Contains("launch") && tweetTags.Contains("ido"));
                            if (!check)
                            {
                                check = tweetFullText.Contains("coming") && tweetFullText.Contains("ido") || (tweetTags.Contains("coming") && tweetTags.Contains("ido"));
                            }
                            if (!check)
                            {
                                check = tweetFullText.Contains("upcoming") && tweetFullText.Contains("ido") || (tweetTags.Contains("upcoming") && tweetTags.Contains("ido"));
                            }
                            break;
                        default:
                            check = false;
                            break;
                    }

                    if (check)
                    {
                        signals.Add(CrawlConsts.Signal.UPCOMMING_TOKEN_SALE);
                    }
                }

                if (mediaMentionedTags.Contains("cmc_cg_new_listing"))
                {
                    if (mediaMentionedUserId == "1688991175867502593")
                    {
                        if (tweetTags.Contains("Coingecko") || tweetTags.Contains("coingecko"))
                        {
                            signals.Add(CrawlConsts.Signal.JUST_LISTED_IN_COINGECKO);
                        }

                        if (tweetTags.Contains("Coinmarketcap") || tweetTags.Contains("coinmarketcap"))
                        {
                            signals.Add(CrawlConsts.Signal.JUST_LISTED_IN_COINMARKETCAP);
                        }
                    }
                }

                if (mediaMentionedTags.Contains("just_raised_funds"))
                {
                    if (mediaMentionedUserId == "1635678642499100672") // screen_name: CryptoRank_VCs      name: Fundraising Digest
                    {
                        if (tweetFullText.StartsWith("⚡️"))
                        {
                            signals.Add(CrawlConsts.Signal.JUST_RAISED_FUNDS);
                        }
                    }
                    else if (mediaMentionedUserId == "1222812013002444800") // screen_name: Crypto_Dealflow      name: Crypto Fundraising
                    {
                        signals.Add(CrawlConsts.Signal.JUST_RAISED_FUNDS);
                    }
                }

                if (mediaMentionedTags.Contains("building_on_bnb_chain"))
                {
                    if (mediaMentionedUserId == "1052454006537314306") // screen_name: BNBCHAIN     name: BNB Chain
                    {
                        if (tweetFullText.ToLower().Contains("welcome"))
                        {
                            signals.Add(CrawlConsts.Signal.BUILDING_ON_BNB_CHAIN);
                        }
                    }
                }
            }
            else // media tag
            {
                if (
                    (tweetTags.Contains("ama") && !tweetFullText.Contains("winner"))
                    || tweetTags.Contains("sponsor")
                    || tweetTags.Contains("sponsored")
                    || tweetTags.Contains("ad")
                    || tweetTags.Contains("ads")
                    )
                {
                    signals.Add(CrawlConsts.Signal.SPONSORED_TWEETS);
                }
                else if (tweetTags.Contains("giveaways")
                        || tweetTags.Contains("airdrops")
                        || tweetTags.Contains("giveaway")
                        || tweetTags.Contains("airdrop")
                        || tweetTags.Contains("gleam"))
                {
                    signals.Add(CrawlConsts.Signal.HOSTING_GIVEAWAYS);
                }
            }

            return signals.Distinct();
        }

        #endregion
    }
}
