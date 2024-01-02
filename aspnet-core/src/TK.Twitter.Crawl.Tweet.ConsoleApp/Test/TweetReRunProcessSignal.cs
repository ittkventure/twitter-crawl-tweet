using System;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Jobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class TweetReRunProcessSignal : ITransientDependency
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
        private readonly TwitterTweetCrawlJob _twitterTweetCrawlJob;

        public TweetReRunProcessSignal(
            IRepository<TwitterTweetEntity, long> tweetRepository,
            IRepository<TwitterInfluencerEntity, long> twitterInfluencerRepository,
            IRepository<TwitterTweetHashTagEntity, long> twitterTweetHashTagRepository,
            IRepository<TwitterTweetMediaEntity, long> twitterTweetMediaRepository,
            IRepository<TwitterTweetSymbolEntity, long> twitterTweetSymbolRepository,
            IRepository<TwitterTweetUrlEntity, long> twitterTweetUrlRepository,
            IRepository<TwitterTweetUserMentionEntity, long> twitterTweetUserMentionRepository,
            IRepository<AirTableNoMentionEntity, long> airTableNoMentionRepository,
            IRepository<AirTableNoMentionWaitingProcessEntity, long> airTableNoMentionWaitingProcessRepository,
            TwitterTweetCrawlJob twitterTweetCrawlJob)
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
            _twitterTweetCrawlJob = twitterTweetCrawlJob;
        }

        public async Task RunAsync()
        {
            var milestone = new DateTime(2023, 12, 29);
            var mentionQuery = await _twitterTweetUserMentionRepository.GetQueryableAsync();

            var tweetQuery = from tweet in await _tweetRepository.GetQueryableAsync()
                                 //where !mentionQuery.Any(x => x.TweetId == tweet.TweetId)
                             where tweet.CreatedAt >= milestone
                             select tweet;

            var tweets = await _tweetRepository.AsyncExecuter.ToListAsync(tweetQuery);
            var tweetTags = await _twitterTweetHashTagRepository.GetListAsync(x => x.CreationTime >= milestone);
            var tweetMentions = await _twitterTweetUserMentionRepository.GetListAsync(x => x.CreationTime >= milestone);
            var influencers = await _twitterInfluencerRepository.GetListAsync();

            var dict = influencers.ToDictionary(x => x.UserId);

            int count = 1;
            foreach (var tweet in tweets)
            {
                Console.WriteLine($"{count} of {tweets.Count}");
                if (!dict.ContainsKey(tweet.UserId))
                {
                    continue;
                }

                var influencer = dict[tweet.UserId];
                var tTags = tweetTags.Where(x => x.TweetId == tweet.TweetId).ToList();
                var tMentions = tweetMentions.Where(x => x.TweetId == tweet.TweetId).ToList();
                if (tMentions.IsNotEmpty())
                {
                    await _twitterTweetCrawlJob.ProcessSignalWithUserMention(new()
                    {
                        BatchKey = "Rerun_2024_01_02",
                        MediaMentionedTags = influencer.Tags,
                        MediaMentionedUserId = influencer.UserId,
                        Tags = tTags,
                        Mentions = tMentions,
                        Tweet = tweet,

                    });
                }
                count++;
            }

        }
    }
}
