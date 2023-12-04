using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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


        public static List<string> IGNORE_USER_SCREENNAME_MENTIONS = new List<string>()
            {
                "binance",
                "coinbase",
                "bnbchain",
                "epicgames",
                "bitfinex",
                "bitmartexchange",
            };

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
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _twitterAPITweetService = twitterAPITweetService;
            _twitterAccountRepository = twitterAccountRepository;
            _memoryCache = memoryCache;
            _distributedEventBus = distributedEventBus;
        }

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

        public async Task AddTweet(string userId, string tweetId, string jsonContent, string kolTags, string batchKey)
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
                UserId = userId,
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

                    foreach (var item in mentions)
                    {
                        if (item.UserId == tweet.UserId) // bỏ qua mention chính nó
                        {
                            continue;
                        }

                        if (item.UserId == "-1") // bỏ qua các mention đến user suspended
                        {
                            continue;
                        }

                        if (IGNORE_USER_SCREENNAME_MENTIONS.Any(x => item.NormalizeScreenName.Contains(x)))// bỏ qua các mention đến user lớn để bú fame
                        {
                            continue;
                        }

                        // loại bỏ thằng này vì tự tag mình vào các bài listing cmc/cgk
                        if (item.UserId == CrawlConsts.TwitterUser.BOT_OWNER_NEW_LISTING_CMC_CGK_USER_ID)
                        {
                            continue;
                        }

                        var signals = GetSignals(userId, tweet.NormalizeFullText, kolTags, tags.Select(x => x.NormalizeText));
                        if (signals.IsNotEmpty())
                        {
                            foreach (var signal in signals)
                            {
                                await _twitterUserSignalRepository.InsertAsync(new TwitterUserSignalEntity()
                                {
                                    UserId = item.UserId,
                                    TweetId = tweet.TweetId,
                                    Signal = signal,
                                    Source = CrawlConsts.Signal.Source.TWITTER_TWEET
                                });
                            }

                            await _leadWaitingProcessEntityRepository.InsertAsync(new LeadWaitingProcessEntity()
                            {
                                BatchKey = batchKey,
                                UserId = item.UserId,
                                TweetId = tweet.TweetId,
                                Source = CrawlConsts.Signal.Source.TWITTER_TWEET
                            });

                            if (LeadProcessWaitingJob.IsLeadBySignalCode(signals))
                            {
                                var userType = await _twitterUserTypeRepository.FirstOrDefaultAsync(x => x.UserId == item.UserId);
                                if (userType == null)
                                {
                                    await _twitterUserTypeRepository.InsertAsync(new TwitterUserTypeEntity()
                                    {
                                        UserId = item.UserId,
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

                            var userStatus = await _twitterUserStatusRepository.FirstOrDefaultAsync(x => x.UserId == item.UserId);
                            if (userStatus == null)
                            {
                                await _twitterUserStatusRepository.InsertAsync(new TwitterUserStatusEntity()
                                {
                                    UserId = item.UserId,
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
                }
            }
        }

        public static IEnumerable<string> GetSignals(string kolUserId, string tweetDescription, string kolTags, IEnumerable<string> hashTags)
        {
            tweetDescription ??= string.Empty;

            List<string> signals = new();
            if (kolTags.IsNotEmpty())
            {
                if (kolTags.Contains("cex"))
                {
                    bool check;
                    switch (kolUserId)
                    {
                        case "978566222282444800": // MEXC_Official
                            check = tweetDescription.Contains("deposit") && tweetDescription.Contains("trading");
                            break;
                        case "912539725071777792": // gate_io
                            check = tweetDescription.Contains("listing") && tweetDescription.Contains("trading");
                            break;
                        case "1098881129057112064": // bitgetglobal
                        case "999947328621395968": // Bybit_Official
                        case "937166242208763904": // BitMartExchange
                            check = tweetDescription.Contains("listing") && tweetDescription.Contains("deposit");
                            break;
                        case "910110294625492992": // kucoincom
                            check = tweetDescription.Contains("listing") && tweetDescription.Contains("new");
                            break;
                        case "957221394009354245": // bitforexcom
                            check = hashTags.Contains("newlistings");
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

                if (kolTags.Contains("audit"))
                {
                    bool check;
                    switch (kolUserId)
                    {
                        case "993673575230996480": // certik
                        case "1571404564540047361": // securewiseAudit
                        case "1390840374": // TechRightio
                            check = tweetDescription.Contains("complete") && tweetDescription.Contains("audit");
                            break;
                        case "1478527011140214784": // contractwolf_io
                            check = tweetDescription.Contains("audit") && tweetDescription.Contains("report");
                            break;
                        case "1370160171822018560": // AssureDefi
                            check = tweetDescription.Contains("assured");
                            break;
                        case "1496534305891311622": // CoinsultAudits
                        case "1461394993537589248": // VB_Audit
                        case "1398253738905714690": // SolidProof_io
                            check = tweetDescription.Contains("audit") && tweetDescription.Contains("complete");
                            break;
                        case "898528774303735808": // hackenclub
                            check = tweetDescription.Contains("audit") && (tweetDescription.Contains("complete") || tweetDescription.Contains("successfully"));
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

                if (kolTags.Contains("upcoming_token_sale"))
                {
                    bool check;
                    switch (kolUserId)
                    {
                        case "1402594955047043074": // Spores_Network
                            check = (tweetDescription.Contains("ido") && tweetDescription.Contains("launching")) || (hashTags.Contains("ido") && hashTags.Contains("launching"));
                            break;
                        case "1171766610031341568": // Kingdomstarter
                            check = (tweetDescription.Contains("ido") && tweetDescription.Contains("commit")) || (hashTags.Contains("ido") && hashTags.Contains("commit"));
                            break;
                        case "1384903770392387586": // bullperks
                            check = (tweetDescription.Contains("ido") && tweetDescription.Contains("upcoming")) || (hashTags.Contains("ido") && hashTags.Contains("upcoming"));
                            break;
                        case "1277204360662077440": // trustswap
                            check = tweetDescription.Contains("new") && tweetDescription.Contains("launchpad") || (hashTags.Contains("new") && hashTags.Contains("launchpad"));

                            if (!check)
                            {
                                check = (tweetDescription.Contains("next") && tweetDescription.Contains("launchpad")) || (hashTags.Contains("next") && hashTags.Contains("launchpad"));
                            }

                            if (!check)
                            {
                                check = (tweetDescription.Contains("upcoming") && tweetDescription.Contains("launchpad")) || (hashTags.Contains("upcoming") && hashTags.Contains("launchpad"));
                            }

                            if (!check)
                            {
                                check = (tweetDescription.Contains("incoming") && tweetDescription.Contains("launchpad")) || (hashTags.Contains("incoming") && hashTags.Contains("launchpad"));
                            }

                            if (!check)
                            {
                                check = (tweetDescription.Contains("introducing") && tweetDescription.Contains("launchpad")) || (hashTags.Contains("introducing") && hashTags.Contains("launchpad"));
                            }
                            break;
                        case "1514973302280048647": // Gateio_Startup
                            check = (tweetDescription.Contains("launchpad")) || (hashTags.Contains("launchpad"));
                            break;
                        case "957221394009354245": // bitforexcom
                            check = (tweetDescription.Contains("ieo")) || (hashTags.Contains("ieo"));
                            break;
                        case "937166242208763904": // BitMartExchange
                            check = (tweetDescription.Contains("launchpad")) || (hashTags.Contains("launchpad"));
                            break;
                        case "1389870631735414787": // kommunitas1
                            check = (tweetDescription.Contains("iko")) || (hashTags.Contains("iko"));
                            break;
                        case "999947328621395968": // Bybit_Official
                            check = ((tweetDescription.Contains("new") && tweetDescription.Contains("launchpad")) && tweetDescription.Contains("project")) || (hashTags.Contains("launchpad") && hashTags.Contains("project") && hashTags.Contains("project"));
                            break;
                        case "1385154488449658880": // pinkecosystem
                            check = (tweetDescription.Contains("launchpad")) || (hashTags.Contains("launchpad"));
                            break;
                        case "1300122153317212161": // Poolz__
                            check = (tweetDescription.Contains("ido") && tweetDescription.Contains("launching")) || (hashTags.Contains("ido") && hashTags.Contains("launching"));
                            break;
                        case "1460254342070550530": // thegempad
                            check = (tweetDescription.Contains("announcement") && tweetDescription.Contains("launch")) || (hashTags.Contains("announcement") && hashTags.Contains("launch"));
                            break;
                        case "1415522287126671363": // GameFi_Official
                            check = tweetDescription.Contains("whitelist") || (hashTags.Contains("whitelist"));
                            break;
                        case "1368509175769092096": // BSClaunchorg
                            check = (tweetDescription.Contains("upcoming") && tweetDescription.Contains("ido")) || (hashTags.Contains("upcoming") && hashTags.Contains("ido"));
                            if (!check)
                            {
                                check = (tweetDescription.Contains("launch") && tweetDescription.Contains("ido")) || (hashTags.Contains("launch") && hashTags.Contains("ido"));
                            }
                            break;
                        case "1421126750214385672": // finblox
                            check = (tweetDescription.Contains("finlaunch") && tweetDescription.Contains("date")) || (hashTags.Contains("finlaunch") && hashTags.Contains("date"));
                            break;
                        case "1345102860732788740": // SeedifyFund                            
                            check = tweetDescription.Contains("launch") && tweetDescription.Contains("ido") || (hashTags.Contains("launch") && hashTags.Contains("ido"));
                            if (!check)
                            {
                                check = tweetDescription.Contains("coming") && tweetDescription.Contains("ido") || (hashTags.Contains("coming") && hashTags.Contains("ido"));
                            }
                            if (!check)
                            {
                                check = tweetDescription.Contains("upcoming") && tweetDescription.Contains("ido") || (hashTags.Contains("upcoming") && hashTags.Contains("ido"));
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

                if (kolTags.Contains("cmc_cg_new_listing"))
                {
                    if (kolUserId == "1688991175867502593")
                    {
                        if (hashTags.Contains("Coingecko") || hashTags.Contains("coingecko"))
                        {
                            signals.Add(CrawlConsts.Signal.JUST_LISTED_IN_COINGECKO);
                        }

                        if (hashTags.Contains("Coinmarketcap") || hashTags.Contains("coinmarketcap"))
                        {
                            signals.Add(CrawlConsts.Signal.JUST_LISTED_IN_COINMARKETCAP);
                        }
                    }
                }
            }
            else // media tag
            {
                if (
                    (hashTags.Contains("ama") && !tweetDescription.Contains("winner"))
                    || hashTags.Contains("sponsor")
                    || hashTags.Contains("sponsored")
                    || hashTags.Contains("ad")
                    || hashTags.Contains("ads")
                    )
                {
                    signals.Add(CrawlConsts.Signal.SPONSORED_TWEETS);
                }
                else if (hashTags.Contains("giveaways")
                        || hashTags.Contains("airdrops")
                        || hashTags.Contains("giveaway")
                        || hashTags.Contains("airdrop")
                        || hashTags.Contains("gleam"))
                {
                    signals.Add(CrawlConsts.Signal.HOSTING_GIVEAWAYS);
                }
            }

            return signals.Distinct();
        }

        public DateTime? GetTweetCreatedAt(JToken entry)
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
    }
}
