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

        public async Task RunAsync_2()
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

        public async Task RunAsync_()
        {
            var tweets = await _tweetRepository.GetListAsync();
            var tweetTags = await _twitterTweetHashTagRepository.GetListAsync();
            var influencers = await _twitterInfluencerRepository.GetListAsync();

            var dict = influencers.ToDictionary(x => x.UserId);

            var noMentionTweets = new Dictionary<string, IEnumerable<string>>();

            var run = async (int taskId, IEnumerable<TwitterTweetEntity> tweets) =>
            {
                int count = 1;
                int total = tweets.Count();
                var noMentionTweets = new Dictionary<string, IEnumerable<string>>();
                foreach (var tweet in tweets)
                {
                    Console.WriteLine($"Task {taskId}: {count} of {total}");
                    if (!dict.ContainsKey(tweet.UserId))
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

            var task1 = Task.Run(() => run(0, tweets.Skip(27500 * 0).Take(27500)));
            var task2 = Task.Run(() => run(1, tweets.Skip(27500 * 1).Take(27500)));
            var task3 = Task.Run(() => run(2, tweets.Skip(27500 * 2).Take(27500)));
            var task4 = Task.Run(() => run(3, tweets.Skip(27500 * 3).Take(27500)));
            var task5 = Task.Run(() => run(4, tweets.Skip(27500 * 4).Take(27500)));
            var task6 = Task.Run(() => run(5, tweets.Skip(27500 * 5).Take(27500)));
            var task7 = Task.Run(() => run(6, tweets.Skip(27500 * 6).Take(27500)));
            var task8 = Task.Run(() => run(7, tweets.Skip(27500 * 7).Take(27500)));

            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8);

            var result = new Dictionary<string, IEnumerable<string>>();

            void func(Task<Dictionary<string, IEnumerable<string>>> r)
            {
                foreach (var item in r.Result)
                {
                    result.Add(item.Key, item.Value);
                }
            }

            func(task1);
            func(task2);
            func(task3);
            func(task4);
            func(task5);
            func(task6);
            func(task7);
            func(task8);
        }

        public static async Task<Dictionary<string, IEnumerable<string>>> GetDictAsync()
        {
            try
            {
                var json = File.ReadAllText("Data/TweetNoMention.json");
                return JsonHelper.Parse<Dictionary<string, IEnumerable<string>>>(json);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}
