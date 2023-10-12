using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Jobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class TestReRunImportSignalData : ITransientDependency
    {
        private readonly IRepository<TwitterTweetCrawlRawEntity, long> _tweetCrawlRawRepository;
        private readonly IRepository<TwitterUserSignalEntity, long> _twitterUserSignalRepository;
        private readonly IRepository<TwitterTweetEntity, long> _twitterTweetRepository;
        private readonly IRepository<TwitterInfluencerEntity, long> _twitterInfluencerRepository;
        private readonly ILogger<TestReRunImportSignalData> _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public TestReRunImportSignalData(
            IRepository<TwitterTweetCrawlRawEntity, long> tweetCrawlRawRepository,
            IRepository<TwitterUserSignalEntity, long> twitterUserSignalRepository,
            IRepository<TwitterTweetEntity, long> twitterTweetRepository,
            IRepository<TwitterInfluencerEntity, long> twitterInfluencerRepository,
            ILogger<TestReRunImportSignalData> logger,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _tweetCrawlRawRepository = tweetCrawlRawRepository;
            _twitterUserSignalRepository = twitterUserSignalRepository;
            _twitterTweetRepository = twitterTweetRepository;
            _twitterInfluencerRepository = twitterInfluencerRepository;
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public IUnitOfWorkManager Get_unitOfWorkManager()
        {
            return _unitOfWorkManager;
        }

        public async Task Test(IUnitOfWorkManager _unitOfWorkManager)
        {
            var tweetQuery = await _twitterTweetRepository.GetQueryableAsync();
            var tweetRawQuery = await _tweetCrawlRawRepository.GetQueryableAsync();
            var influencerQuery = await _twitterInfluencerRepository.GetQueryableAsync();
            var signalQuery = await _twitterUserSignalRepository.GetQueryableAsync();

            var query = from tweetRaw in tweetRawQuery
                        join tweet in tweetQuery on tweetRaw.TweetId equals tweet.TweetId
                        join influencer in influencerQuery on tweet.UserId equals influencer.UserId
                        where !influencerQuery.Any(x => x.UserId == tweet.UserId && (x.Tags.Contains("cex") || x.Tags.Contains("audit"))) // loại bỏ các tweet của các acc cex/audit vì đã import khi chạy job crawl trước đó
                        where !signalQuery.Any(x => x.TweetId == tweet.TweetId) // loại bỏ các tweet đã đồng bộ trước đó
                        select new
                        {
                            JsonContent = tweetRaw.JsonContent,
                            influencer.Tags,
                            tweet
                        };

            var totalCount = query.Count();
            int take = 1000;
            for (int skip = 0; skip < totalCount; skip += take)
            {
                _logger.LogInformation($"Skip: {skip}. Total Count: {totalCount}");

                var itemsOnPage = query.Skip(skip).Take(take).ToList();

                using var uow = _unitOfWorkManager.Begin();
                try
                {
                    foreach (var raw in itemsOnPage)
                    {
                        try
                        {
                            string fullText = string.Empty;
                            string fullTextNormalize = string.Empty;

                            var entry = JObject.Parse(raw.JsonContent);
                            var content = entry["content"];
                            var itemContent = content["itemContent"];
                            var tweetResult = itemContent["tweet_results"]["result"];

                            var result_type = itemContent["tweet_results"]["result"]["__typename"].ParseIfNotNull<string>();
                            if (result_type == "TweetWithVisibilityResults")
                            {
                                tweetResult = tweetResult["tweet"];
                            }

                            var tweetLegacy = tweetResult["legacy"];

                            var retweetdStatusResult = tweetLegacy["retweeted_status_result"];
                            if (retweetdStatusResult != null)
                            {
                                if (retweetdStatusResult["result"]?["__typename"].ParseIfNotNull<string>() == "TweetWithVisibilityResults")
                                {
                                    fullText = retweetdStatusResult["result"]?["tweet"]?["legacy"]?["full_text"].Value<string>();
                                }
                                else
                                {
                                    fullText = retweetdStatusResult["result"]?["legacy"]?["full_text"].Value<string>();
                                }
                            }
                            else
                            {
                                fullText = tweetLegacy["full_text"].Value<string>();
                            }

                            if (fullText != null)
                            {
                                fullTextNormalize = fullText?.ToLower();
                            }

                            if (raw.tweet.FullText != fullText || raw.tweet.NormalizeFullText != fullTextNormalize)
                            {
                                raw.tweet.FullText = fullText;
                                raw.tweet.NormalizeFullText = fullTextNormalize;
                                await _twitterTweetRepository.UpdateAsync(raw.tweet);
                            }

                            if (tweetLegacy["entities"] != null)
                            {
                                var mentions = new List<Tuple<string, string>>();
                                if (tweetLegacy["entities"]["user_mentions"] != null)
                                {
                                    foreach (var item in tweetLegacy["entities"]["user_mentions"])
                                    {
                                        var id_str = item["id_str"].ParseIfNotNull<string>();
                                        var name = item["name"].ParseIfNotNull<string>();
                                        var screen_name = item["screen_name"].ParseIfNotNull<string>();

                                        mentions.Add(new Tuple<string, string>(id_str, screen_name.ToLower()));
                                    }
                                }

                                var tags = new List<string>();
                                if (tweetLegacy["entities"]["hashtags"] != null)
                                {
                                    foreach (var item in tweetLegacy["entities"]["hashtags"])
                                    {
                                        var text = item["text"].ParseIfNotNull<string>();
                                        tags.Add(text.ToLower());
                                    }
                                }

                                if (mentions.IsNotEmpty())
                                {
                                    foreach (var keypair in mentions)
                                    {
                                        string userId = keypair.Item1;
                                        string normalizeScreenName = keypair.Item2;

                                        if (userId == raw.tweet.UserId) // bỏ qua mention chính nó
                                        {
                                            continue;
                                        }

                                        if (userId == "-1") // bỏ qua các mention đến user suspended
                                        {
                                            continue;
                                        }

                                        if (TwitterTweetCrawlJob.IGNORE_USER_SCREENNAME_MENTIONS.Any(x => normalizeScreenName.Contains(x)))// bỏ qua các mention đến user lớn để bú fame
                                        {
                                            continue;
                                        }

                                        var signals = TwitterTweetCrawlJob.GetSignals(raw.tweet.UserId, fullTextNormalize, raw.Tags, tags);
                                        if (signals.IsNotEmpty())
                                        {
                                            foreach (var signal in signals)
                                            {
                                                await _twitterUserSignalRepository.InsertAsync(new TwitterUserSignalEntity()
                                                {
                                                    UserId = userId,
                                                    TweetId = raw.tweet.TweetId,
                                                    Signal = signal,
                                                    Source = CrawlConsts.Signal.Source.TWITTER_TWEET
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error when update Tweet ID: " + raw.tweet.TweetId);
                        }
                    }

                    await uow.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error");
                    await uow.RollbackAsync();
                }
            }
        }
    }
}
