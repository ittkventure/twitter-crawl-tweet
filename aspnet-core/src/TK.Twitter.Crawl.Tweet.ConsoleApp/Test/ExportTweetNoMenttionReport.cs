using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Threading.Tasks;
using Telegram.Bot.Requests;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Jobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Repositories;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class ExportTweetNoMenttionReport : ITransientDependency
    {
        private readonly IRepository<TwitterTweetEntity, long> _tweetRepository;
        private readonly IRepository<TwitterInfluencerEntity, long> _twitterInfluencerRepository;
        private readonly IRepository<TwitterTweetHashTagEntity, long> _twitterTweetHashTagRepository;
        private readonly IRepository<TwitterTweetMediaEntity, long> _twitterTweetMediaRepository;
        private readonly IRepository<TwitterTweetSymbolEntity, long> _twitterTweetSymbolRepository;
        private readonly IRepository<TwitterTweetUrlEntity, long> _twitterTweetUrlRepository;
        private readonly IRepository<TwitterTweetUserMentionEntity, long> _twitterTweetUserMentionRepository;
        private readonly IRepository<AirTableNoMentionEntity, long> _airTableNoMentionRepository;
        private readonly IRepository<AirTableNoMentionWaitingProcessEntity, long> _airTableNoMentionWaitingProcessRepository;

        public ExportTweetNoMenttionReport(
            IRepository<TwitterTweetEntity, long> tweetRepository,
            IRepository<TwitterInfluencerEntity, long> twitterInfluencerRepository,
            IRepository<TwitterTweetHashTagEntity, long> twitterTweetHashTagRepository,
            IRepository<TwitterTweetMediaEntity, long> twitterTweetMediaRepository,
            IRepository<TwitterTweetSymbolEntity, long> twitterTweetSymbolRepository,
            IRepository<TwitterTweetUrlEntity, long> twitterTweetUrlRepository,
            IRepository<TwitterTweetUserMentionEntity, long> twitterTweetUserMentionRepository,
            IRepository<AirTableNoMentionEntity, long> airTableNoMentionRepository,
            IRepository<AirTableNoMentionWaitingProcessEntity, long> airTableNoMentionWaitingProcessRepository)
        {
            _tweetRepository = tweetRepository;
            _twitterInfluencerRepository = twitterInfluencerRepository;
            _twitterTweetHashTagRepository = twitterTweetHashTagRepository;
            _twitterTweetMediaRepository = twitterTweetMediaRepository;
            _twitterTweetSymbolRepository = twitterTweetSymbolRepository;
            _twitterTweetUrlRepository = twitterTweetUrlRepository;
            _twitterTweetUserMentionRepository = twitterTweetUserMentionRepository;
            _airTableNoMentionRepository = airTableNoMentionRepository;
            _airTableNoMentionWaitingProcessRepository = airTableNoMentionWaitingProcessRepository;
        }

        public class TweetDto
        {
            public string TweetId { get; set; }

            public string TweetUrl { get; set; }

            public string ProfileUrl { get; set; }

            public DateTime? CreatedAt { get; set; }

            public string FullText { get; set; }

            public string Tags { get; set; }

            public string Medias { get; set; }

            public string Urls { get; set; }

            public string Symbols { get; set; }

            public string Mentions { get; set; }

            public string Signals { get; set; }

        }

        public async Task RunAsync()
        {
            await RunAsync_AddNoMentionWaitingProcess();
        }

        public async Task RunAsync_AddNoMentionWaitingProcess()
        {
            var dict = await GetDictAsync();
            foreach (var tweetId in dict.Keys)
            {
                await _airTableNoMentionWaitingProcessRepository.InsertAsync(new AirTableNoMentionWaitingProcessEntity()
                {
                    Action = "PUSH",
                    RefId = tweetId,
                    Signals = dict[tweetId].JoinAsString(",")
                });
            }
        }

        public async Task RunAsync_GetReport()
        {
            var dict = await GetDictAsync();

            var mentionQuery = await _twitterTweetUserMentionRepository.GetQueryableAsync();
            var query = from tweet in await _tweetRepository.GetQueryableAsync()
                        where dict.Keys.Contains(tweet.TweetId) && !mentionQuery.Any(x => x.TweetId == tweet.TweetId)
                        select new TweetDto
                        {
                            TweetId = tweet.TweetId,
                            TweetUrl = "https://twitter.com/_/status/" + tweet.TweetId,
                            ProfileUrl = "https://twitter.com/" + tweet.UserScreenName,
                            CreatedAt = tweet.CreatedAt,
                            FullText = tweet.FullText,
                            Tags = string.Empty,
                            Medias = string.Empty,
                            Symbols = string.Empty,
                            Urls = string.Empty,
                            Mentions = string.Empty
                        };

            var tweets = await _tweetRepository.AsyncExecuter.ToListAsync(query);

            var tags = await _twitterTweetHashTagRepository.GetListAsync(x => dict.Keys.Contains(x.TweetId));
            var medias = await _twitterTweetMediaRepository.GetListAsync(x => dict.Keys.Contains(x.TweetId));
            var symbols = await _twitterTweetSymbolRepository.GetListAsync(x => dict.Keys.Contains(x.TweetId));
            var urls = await _twitterTweetUrlRepository.GetListAsync(x => dict.Keys.Contains(x.TweetId));
            //var mentions = await _twitterTweetUserMentionRepository.GetListAsync(x => dict.Keys.Contains(x.TweetId));

            foreach (var tweet in tweets)
            {
                tweet.FullText = tweet.FullText.Replace("\n", "- ");
                tweet.Tags = tags.Where(x => x.TweetId == tweet.TweetId).Select(x => x.NormalizeText).JoinAsString(";");
                tweet.Symbols = symbols.Where(x => x.TweetId == tweet.TweetId).Select(x => x.SymbolText).JoinAsString(";");
                tweet.Medias = medias.Where(x => x.TweetId == tweet.TweetId).Select(x => $"{x.Type}: {x.MediaUrlHttps}").JoinAsString(";");
                tweet.Urls = urls.Where(x => x.TweetId == tweet.TweetId).Select(x => $"{x.ExpandedUrl}").JoinAsString(";");
                //tweet.Mentions = mentions.Where(x => x.TweetId == tweet.TweetId).Select(x => $"{x.UserId} - {x.ScreenName}").JoinAsString(";");
                tweet.Signals = dict[tweet.TweetId].JoinAsString(";");
            }
        }

        public async Task RunAsync_GetNoMentionTweet()
        {
            var milestone = new DateTime(2022, 12, 26);
            var mentionQuery = await _twitterTweetUserMentionRepository.GetQueryableAsync();

            var tweetQuery = from tweet in await _tweetRepository.GetQueryableAsync()
                             where !mentionQuery.Any(x => x.TweetId == tweet.TweetId)
                             where tweet.CreatedAt >= milestone
                             select tweet;

            var tweets = await _tweetRepository.AsyncExecuter.ToListAsync(tweetQuery);
            var tweetTags = await _twitterTweetHashTagRepository.GetListAsync(x => x.CreationTime >= milestone);
            var influencers = await _twitterInfluencerRepository.GetListAsync();

            var dict = influencers.ToDictionary(x => x.UserId);

            //var done = await GetDictAsync();

            var run = async (int taskId, IEnumerable<TwitterTweetEntity> tweets) =>
            {
                int count = 1;
                int total = tweets.Count();
                var noMentionTweets = new Dictionary<string, IEnumerable<string>>();
                foreach (var tweet in tweets)
                {
                    Console.WriteLine($"Task {taskId}: {count} of {total}");
                    if (!dict.ContainsKey(tweet.UserId)
                    //|| done.ContainsKey(tweet.TweetId)
                    )
                    {
                        continue;
                    }

                    var influencer = dict[tweet.UserId];
                    var signals = TwitterTweetCrawlJob.GetSignals(tweet.UserId, tweet.NormalizeFullText, influencer.Tags, tweetTags.Where(x => x.TweetId == tweet.TweetId).Select(x => x.NormalizeText));
                    if (signals.IsNotEmpty())
                    {
                        noMentionTweets.Add(tweet.TweetId, signals);
                    }
                    count++;
                }

                return await Task.FromResult(noMentionTweets);
            };

            var calculationTake = tweets.Count / 8;

            var tasks = new List<Task<Dictionary<string, IEnumerable<string>>>>();
            for (int i = 1; i <= 8; i++)
            {
                int taskId = i;
                int skip = calculationTake * (i - 1);
                int realTake = calculationTake;
                if (i == 8)
                {
                    realTake += 100000;
                }
                tasks.Add(Task.Run(() => run(taskId, tweets.Skip(skip).Take(realTake))));
            }

            var whenAllCompleteResult = await Task.WhenAll(tasks);
            var noMentionTweets = new Dictionary<string, IEnumerable<string>>();
            foreach (var tresult in whenAllCompleteResult)
            {
                foreach (var item in tresult)
                {
                    noMentionTweets.Add(item.Key, item.Value);
                }
            }
        }

        public static async Task<Dictionary<string, IEnumerable<string>>> GetDictAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync("Data/TweetNoMention.json");
                return JsonHelper.Parse<Dictionary<string, IEnumerable<string>>>(json);
            }
            catch
            {

            }
            return null;
        }
    }
}
