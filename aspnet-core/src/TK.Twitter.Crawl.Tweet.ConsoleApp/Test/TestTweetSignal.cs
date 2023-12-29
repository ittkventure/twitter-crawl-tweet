using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Jobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{
    public class TestTweetSignal : ITransientDependency
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
        private readonly TwitterTweetCrawlJob _tweetCrawlJob;

        public TestTweetSignal(
            IRepository<TwitterTweetEntity, long> tweetRepository,
            IRepository<TwitterInfluencerEntity, long> twitterInfluencerRepository,
            IRepository<TwitterTweetHashTagEntity, long> twitterTweetHashTagRepository,
            IRepository<TwitterTweetMediaEntity, long> twitterTweetMediaRepository,
            IRepository<TwitterTweetSymbolEntity, long> twitterTweetSymbolRepository,
            IRepository<TwitterTweetUrlEntity, long> twitterTweetUrlRepository,
            IRepository<TwitterTweetUserMentionEntity, long> twitterTweetUserMentionRepository,
            IRepository<AirTableNoMentionEntity, long> airTableNoMentionRepository,
            IRepository<AirTableNoMentionWaitingProcessEntity, long> airTableNoMentionWaitingProcessRepository,
            TwitterTweetCrawlJob tweetCrawlJob)
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
            _tweetCrawlJob = tweetCrawlJob;
        }

        public async Task RunAsync()
        {
            const string TWEET_ID = "1740239115596300688";
            const string MEDIA_MENTIONED_TAGS = "";
            const string MEDIA_MENTIONED_USER_ID = "1514276588804001793";

            var tweet = await _tweetRepository.FirstOrDefaultAsync(x => x.TweetId == TWEET_ID);
            var tweetMentions = await _twitterTweetUserMentionRepository.GetListAsync(x => x.TweetId == TWEET_ID);
            var tags = await _twitterTweetHashTagRepository.GetListAsync(x => x.TweetId == TWEET_ID);

            await _tweetCrawlJob.ProcessSignalWithUserMention(new()
            {
                BatchKey = null,
                MediaMentionedTags = MEDIA_MENTIONED_TAGS,
                MediaMentionedUserId = MEDIA_MENTIONED_USER_ID,
                Tweet = tweet,
                Mentions = tweetMentions,
                Tags = tags
            });
        }
    }
}
