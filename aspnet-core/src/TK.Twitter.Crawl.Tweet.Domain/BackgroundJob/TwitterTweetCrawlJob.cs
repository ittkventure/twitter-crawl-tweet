﻿using Medallion.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Entity.Dapper;
using TK.Twitter.Crawl.Repository;
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
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly TwitterAPITweetService _twitterAPITweetService;
        private readonly ITwitterAccountRepository _twitterAccountRepository;
        private readonly IDistributedLockProvider _distributedLockProvider;

        public TwitterTweetCrawlJob(
            IBackgroundJobManager backgroundJobManager,
            IRepository<TwitterTweetCrawlQueueEntity, long> twitterFollowingCrawlQueueRepository,
            IRepository<TwitterTweetCrawlRawEntity, long> twitterTweetCrawlRawRepository,
            IRepository<TwitterTweetEntity, long> twitterTweetRepository,
            IRepository<TwitterTweetMediaEntity, long> twitterTweetMediaRepository,
            IRepository<TwitterTweetUserMentionEntity, long> twitterTweetUserMentionRepository,
            IRepository<TwitterTweetHashTagEntity, long> twitterTweetHashTagRepository,
            IRepository<TwitterTweetUrlEntity, long> twitterTweetUrlRepository,
            IRepository<TwitterTweetSymbolEntity, long> twitterTweetSymbolRepository,
            IClock clock,
            IUnitOfWorkManager unitOfWorkManager,
            TwitterAPITweetService twitterAPITweetService,
            ITwitterAccountRepository twitterAccountRepository,
            IDistributedLockProvider distributedLockProvider)
        {
            _backgroundJobManager = backgroundJobManager;
            _twitterTweetCrawlQueueRepository = twitterFollowingCrawlQueueRepository;
            _twitterTweetCrawlRawRepository = twitterTweetCrawlRawRepository;
            _twitterTweetRepository = twitterTweetRepository;
            _twitterTweetMediaRepository = twitterTweetMediaRepository;
            _twitterTweetUserMentionRepository = twitterTweetUserMentionRepository;
            _twitterTweetHashTagRepository = twitterTweetHashTagRepository;
            _twitterTweetUrlRepository = twitterTweetUrlRepository;
            _twitterTweetSymbolRepository = twitterTweetSymbolRepository;
            _clock = clock;
            _unitOfWorkManager = unitOfWorkManager;
            _twitterAPITweetService = twitterAPITweetService;
            _twitterAccountRepository = twitterAccountRepository;
            _distributedLockProvider = distributedLockProvider;
        }

        [UnitOfWork(IsDisabled = true)]
        public override async Task ExecuteAsync(TwitterTweetCrawlJobArg args)
        {
            await using (var handle = await _distributedLockProvider.TryAcquireLockAsync($"TwitterFollowingCrawlJob_{args.BatchKey}_{args.TwitterAccountId}"))
            {
                if (handle == null)
                {
                    Logger.LogInformation(LOG_PREFIX + "Some process is running!!!");
                    return;
                }

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
                        return;
                    }

                    var crawlAccount = await _twitterAccountRepository.FirstOrDefaultAsync(x => x.AccountId == args.TwitterAccountId);
                    if (crawlAccount == null)
                    {
                        throw new BusinessException(CrawlDomainErrorCodes.NotFound, $"Crawl Account {args.TwitterAccountId} not found");
                    }

                    foreach (var item in queues)
                    {
                        var relationShips = new List<TwitterTweetCrawlTweetDapperEntity>();

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
                                //if (loop == 3)
                                //{
                                //    succeedGet = true;
                                //    messageGet = "Succeed";
                                //    break;
                                //}

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

                                        await AddTweet(item.UserId, data.TweetId, data.JsonContent);

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
            }

            await _backgroundJobManager.EnqueueAsync(args);
        }

        public async Task AddTweet(string userId, string tweetId, string jsonContent)
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
            tweet.FullText = tweetLegacy["full_text"].Value<string>();
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

                if (tweetLegacy["entities"]["user_mentions"] != null)
                {
                    foreach (var item in tweetLegacy["entities"]["user_mentions"])
                    {
                        var id_str = item["id_str"].ParseIfNotNull<string>();
                        var name = item["name"].ParseIfNotNull<string>();
                        var screen_name = item["screen_name"].ParseIfNotNull<string>();

                        await _twitterTweetUserMentionRepository.InsertAsync(new TwitterTweetUserMentionEntity()
                        {
                            TweetId = tweetId,
                            UserId = id_str,
                            Name = name,
                            ScreenName = screen_name,
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

                if (tweetLegacy["entities"]["hashtags"] != null)
                {
                    foreach (var item in tweetLegacy["entities"]["hashtags"])
                    {
                        var text = item["text"].ParseIfNotNull<string>();
                        await _twitterTweetHashTagRepository.InsertAsync(new TwitterTweetHashTagEntity()
                        {
                            TweetId = tweetId,
                            Text = text,
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
            }
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
    }
}
