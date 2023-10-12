using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Entity.Dapper;
using TK.Twitter.Crawl.TwitterAPI;
using TK.Twitter.Crawl.TwitterAPI.Dto;
using TK.TwitterAccount.Domain;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class TestTwitterGetTweet : ITransientDependency
    {
        private readonly TwitterAPITweetService _twitterAPITweetService;
        private readonly TwitterAPIUserService _twitterAPIUserService;
        private readonly IClock _clock;
        private readonly TwitterAPIAuthService _twitterAPIAuthService;
        private readonly ITwitterAccountRepository _twitterAccountRepository;
        private readonly IRepository<TwitterCrawlAccountEntity, long> _twitterCrawlAccountRepository;
        private readonly IRepository<TwitterTweetEntity, long> _twitterTweetRepository;

        public TestTwitterGetTweet(
            TwitterAPITweetService twitterAPITweetService,
            TwitterAPIUserService twitterAPIUserService,
            IClock clock,
            TwitterAPIAuthService twitterAPIAuthService,
            ITwitterAccountRepository twitterAccountRepository,
            IRepository<TwitterCrawlAccountEntity, long> twitterCrawlAccountRepository,
            IRepository<TwitterTweetEntity, long> twitterTweetRepository)
        {
            _twitterAPITweetService = twitterAPITweetService;
            _twitterAPIUserService = twitterAPIUserService;
            _clock = clock;
            _twitterAPIAuthService = twitterAPIAuthService;
            _twitterAccountRepository = twitterAccountRepository;
            _twitterCrawlAccountRepository = twitterCrawlAccountRepository;
            _twitterTweetRepository = twitterTweetRepository;
        }

        public async Task Test()
        {
            var response = await _twitterAPIUserService.GetUserByScreenNameAsync("b2binpay", "Account_5");
            
        }

        public async Task Test_1()
        {
            try
            {
                var entries = new List<TwitterAPITweetDto>();
                string messageGet = string.Empty;
                bool succeedGet = false;
                string cursor = null;
                string accountId = "Account_5";
                int loop = 0;
                string userId = "1108654113095479296";

                while (true)
                {
                    loop++;
                    if (loop == 11)
                    {
                        break;
                    }

                    TwitterAPIGetTweetResponse response = null;
                    try
                    {
                        response = await _twitterAPITweetService.GetTweetAsync(userId, accountId, cursor: cursor);
                        if (response.RateLimit > 0 || response.TooManyRequest)
                        {
                            var subtract = response.RateLimitResetAt.Value.Subtract(_clock.Now);
                            if (response.RateLimitRemaining <= 1)
                            {
                                await Task.Delay(subtract);
                                response = await _twitterAPITweetService.GetTweetAsync(userId, accountId, cursor: cursor);
                            }
                        }
                    }
                    catch (BusinessException ex)
                    {
                        if (ex.Code == CrawlDomainErrorCodes.TwitterAuthorizationError)
                        {
                            // chỉ cho login lại 1 lần
                            response = await _twitterAPITweetService.GetTweetAsync(userId, accountId, cursor: cursor);
                        }
                        else
                        {
                            succeedGet = false;
                            messageGet = ex.Message;
                            break;
                        }
                    }

                    JObject jsonContent = JObject.Parse(response.JsonText);

                    if ((string)jsonContent["data"]["user"]["result"]["typename"] == "UserUnavailable")
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

                    var tweetInPages = new List<TwitterAPITweetDto>();
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

                        var tweetResult = itemContent["tweet_results"]["result"];

                        var tweet = new TwitterAPITweetDto()
                        {
                            UserId = userId,
                            TweetId = tweetResult["rest_id"].Value<string>()
                        };

                        tweetInPages.Add(tweet);

                        if (tweetResult["core"]["user_results"]["result"]["legacy"] != null)
                        {
                            tweet.UserResultAsJson = JsonHelper.Stringify(tweetResult["core"]["user_results"]["result"]["legacy"]);
                        }

                        if (tweetResult["views"]["count"] != null)
                        {
                            tweet.ViewsCount = tweetResult["views"]["count"].Value<int>();
                        }

                        if (tweetResult["quoted_status_result"] != null)
                        {
                            tweet.QuoteStatusResultAsJson = JsonHelper.Stringify(tweetResult["quoted_status_result"]);
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

                        if (tweetLegacy["entities"] != null)
                        {
                            tweet.EntitiesAsJson = JsonHelper.Stringify(tweetLegacy["entities"]);
                        }

                        if (tweetLegacy["extended_entities"] != null)
                        {
                            tweet.ExtendedEntitiesAsJson = JsonHelper.Stringify(tweetLegacy["extended_entities"]);
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
                    }

                    if (tweetInPages.IsEmpty())
                    {
                        succeedGet = true;
                        messageGet = "Succeed";
                        break;
                    }

                    entries.AddRange(tweetInPages);
                }

                if (entries.IsNotEmpty())
                {
                    var tweets = new List<TwitterTweetEntity>();
                    foreach (var tweet in entries)
                    {
                        tweets.Add(new TwitterTweetEntity
                        {
                            UserId = tweet.UserId,
                            BookmarkCount = tweet.BookmarkCount,
                            ConversationId = tweet.ConversationId,
                            CreatedAt = tweet.CreatedAt,
                            FavoriteCount = tweet.FavoriteCount,
                            FullText = tweet.FullText,
                            InReplyToScreenName = tweet.InReplyToScreenName,
                            InReplyToStatusId = tweet.InReplyToStatusId,
                            InReplyToUserId = tweet.InReplyToUserId,
                            IsQuoteStatus = tweet.IsQuoteStatus,
                            Lang = tweet.Lang,
                            QuoteCount = tweet.QuoteCount,
                            ReplyCount = tweet.ReplyCount,
                            ViewsCount = tweet.ViewsCount,
                            TweetId = tweet.TweetId,
                            RetweetCount = tweet.RetweetCount
                        });
                    }

                    await _twitterTweetRepository.InsertManyAsync(tweets);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
