using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Entity.Dapper;
using TK.Twitter.Crawl.Repository;
using Volo.Abp.Domain.Repositories;
using static System.Collections.Specialized.BitVector32;

namespace TK.Twitter.Crawl.Twitter
{

    public class TweetAppService : CrawlAppService
    {
        private readonly IRepository<TwitterTweetEntity, long> _tweetRepository;
        private readonly IRepository<TwitterUserTypeEntity, long> _tweetUserTypeRepository;
        private readonly IRepository<TwitterUserStatusEntity, long> _tweetUserStatusRepository;
        private readonly IRepository<TwitterTweetUserMentionEntity, long> _tweetUserMentionRepository;
        private readonly IRepository<TwitterTweetUrlEntity, long> _tweetUrlRepository;
        private readonly IRepository<TwitterTweetSymbolEntity, long> _tweetSymbolRepository;
        private readonly IRepository<TwitterTweetMediaEntity, long> _tweetMediaRepository;
        private readonly IRepository<TwitterTweetHashTagEntity, long> _tweetHashTagRepository;
        private readonly ITwitterTweetMentionDapperRepository _twitterTweetMentionDapperRepository;

        public TweetAppService(
            IRepository<TwitterTweetEntity, long> tweetRepository,
            IRepository<TwitterUserTypeEntity, long> tweetUserTypeRepository,
            IRepository<TwitterUserStatusEntity, long> tweetUserStatusRepository,
            IRepository<TwitterTweetUserMentionEntity, long> tweetUserMentionRepository,
            IRepository<TwitterTweetUrlEntity, long> tweetUrlRepository,
            IRepository<TwitterTweetSymbolEntity, long> tweetSymbolRepository,
            IRepository<TwitterTweetMediaEntity, long> tweetMediaRepository,
            IRepository<TwitterTweetHashTagEntity, long> tweetHashTagRepository,
            ITwitterTweetMentionDapperRepository twitterTweetMentionDapperRepository)
        {
            _tweetRepository = tweetRepository;
            _tweetUserTypeRepository = tweetUserTypeRepository;
            _tweetUserStatusRepository = tweetUserStatusRepository;
            _tweetUserMentionRepository = tweetUserMentionRepository;
            _tweetUrlRepository = tweetUrlRepository;
            _tweetSymbolRepository = tweetSymbolRepository;
            _tweetMediaRepository = tweetMediaRepository;
            _tweetHashTagRepository = tweetHashTagRepository;
            _twitterTweetMentionDapperRepository = twitterTweetMentionDapperRepository;
        }

        public async Task<PagingResult<TweetMentionDto>> GetMentionListAsync(int pageNumber, int pageSize, string userStatus, string userType, string searchText, string ownerUserScreenName)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            var mentionQuery = await _tweetUserMentionRepository.GetQueryableAsync();
            var hashTagQuery = await _tweetHashTagRepository.GetQueryableAsync();
            var tweetQuery = await _tweetRepository.GetQueryableAsync();

            var ignoreScreenName = new List<string>()
            {
                "binance",
                "coinbase",
                "bnbchain",
                "epicgames",
                "bitfinex",
                "bitmartexchange",
            };

            var mentionWithFilterQuery = (
                                          from mention in mentionQuery
                                          join tweet in tweetQuery on mention.TweetId equals tweet.TweetId
                                          join hashTagOriginal in hashTagQuery on tweet.TweetId equals hashTagOriginal.TweetId
                                          into hashTag
                                          from ht in hashTag.DefaultIfEmpty()
                                          where (ht.Text != "ama" && !tweet.NormalizeFullText.Contains("winner"))
                                          && mention.UserId != "-1"
                                          && mention.UserId != tweet.UserId
                                          && !ignoreScreenName.Contains(mention.NormalizeScreenName)
                                          select new
                                          {
                                              mention,
                                          }

                                      ).GroupBy(x => x.mention.UserId).Select(mt => new
                                      {
                                          UserId = mt.Key,
                                          MaxCreationTime = mt.Max(x => x.mention.CreationTime),
                                          NumberOfSponsoredTweets = mt.Count(),
                                      });


            var tweetWithMentionCountQuery = from tweet in tweetQuery
                                             join mention in mentionQuery.GroupBy(x => x.TweetId).Select(x => new { TweetId = x.Key, MentionCount = x.Count() }) on tweet.TweetId equals mention.TweetId
                                             select new
                                             {
                                                 tweet,
                                                 mention.MentionCount
                                             };

            var query = from mention_main in mentionQuery
                        join mention_filter in mentionWithFilterQuery on mention_main.CreationTime equals mention_filter.MaxCreationTime

                        join user_type_origin in await _tweetUserTypeRepository.GetQueryableAsync() on mention_main.UserId equals user_type_origin.UserId
                        into user_type_temp
                        from user_type in user_type_temp.DefaultIfEmpty()

                        join user_status_origin in await _tweetUserStatusRepository.GetQueryableAsync() on mention_main.UserId equals user_status_origin.UserId
                        into user_status_temp
                        from user_status in user_status_temp.DefaultIfEmpty()

                        where mention_main.UserId == mention_filter.UserId

                        select new
                        {
                            mention_main,
                            user_status.Status,
                            user_type.Type,
                            mention_filter.NumberOfSponsoredTweets,
                        };

            //query = from q in query
            //        join last_tweet in tweetWithMentionCountQuery on q.mention_main.TweetId equals last_tweet.tweet.TweetId

            //        where last_tweet.tweet.CreatedAt != null
            //        orderby last_tweet.tweet.CreatedAt descending

            //        select new
            //        {
            //            q.mention_main,
            //            q.Status,
            //            q.Type,
            //            q.NumberOfSponsoredTweets,                        
            //        };

            query = query.WhereIf(userStatus.IsNotEmpty(), x => x.Status == userStatus);
            query = query.WhereIf(userType.IsNotEmpty(), x => x.Type == userType);

            query = query.OrderByDescending(x => x.mention_main.CreationTime);
            
            query = query.WhereIf(ownerUserScreenName.IsNotEmpty(), x => tweetWithMentionCountQuery.Any(x => x.tweet.UserScreenNameNormalize == ownerUserScreenName));
            query = query.WhereIf(searchText.IsNotEmpty(), x => x.mention_main.NormalizeScreenName.Contains(searchText.ToLower())
                                                                || tweetWithMentionCountQuery.Any(x => x.tweet.FullText.Contains(searchText.ToLower())));

            var pr = new PagingResult<TweetMentionDto>();
            pr.TotalCount = await AsyncExecuter.CountAsync(query);
            if (pr.TotalCount == 0)
            {
                return pr;
            }

            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            pr.Items = await AsyncExecuter.ToListAsync(query.Select(q => new TweetMentionDto()
            {
                UserId = q.mention_main.UserId,
                LastestTweetId = q.mention_main.TweetId,
                UserName = q.mention_main.Name,
                UserScreenName = q.mention_main.ScreenName,
                UserType = q.Type,
                UserStatus = q.Status,
                NumberOfSponsoredTweets = q.NumberOfSponsoredTweets,
            }));

            var tags = await _tweetHashTagRepository.GetListAsync(x => pr.Items.Select(x => x.LastestTweetId).Contains(x.TweetId));
            var lastestTweets = await AsyncExecuter.ToListAsync(tweetWithMentionCountQuery.Where(x => pr.Items.Select(x => x.LastestTweetId).Contains(x.tweet.TweetId)));

            foreach (var item in pr.Items)
            {
                var itemTags = tags.Where(x => x.TweetId == item.LastestTweetId);
                item.HashTags = itemTags.Select(x => x.Text).Distinct().ToList();

                if (item.UserStatus.IsEmpty())
                {
                    item.UserStatus = "New";
                }

                var tweet = lastestTweets.FirstOrDefault(x => x.tweet.TweetId == item.LastestTweetId);
                item.LastestSponsoredDate = tweet?.tweet.CreatedAt;
                item.TweetDescription = tweet?.tweet.FullText;
                item.TweetOwnerUserId = tweet?.tweet.UserId;
                item.MediaMentioned = tweet?.tweet.UserScreenNameNormalize;
                item.DuplicateUrlCount = tweet?.MentionCount;

                item.LastestSponsoredTweetUrl = "https://twitter.com/_/status/" + item.LastestTweetId; // url k cần quan tâm đên username nên thay bằng _
            }

            return pr;
        }

        public async Task<PagingResult<TweetDto>> GetTweetListAsync([Required] string userId, int pageNumber, int pageSize, string searchText)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            var query = from tweet in await _tweetRepository.GetQueryableAsync()
                        join mention in await _tweetUserMentionRepository.GetQueryableAsync() on tweet.TweetId equals mention.TweetId
                        where mention.UserId == userId
                        orderby tweet.CreatedAt descending
                        select tweet;

            query = query.WhereIf(searchText.IsNotEmpty(), x => x.NormalizeFullText.Contains(searchText));

            var pr = new PagingResult<TweetDto>();
            pr.TotalCount = await AsyncExecuter.CountAsync(query);
            if (pr.TotalCount == 0)
            {
                return pr;
            }

            pr.Items = await AsyncExecuter.ToListAsync(
                   query.Select(x => new TweetDto()
                   {
                       TweetId = x.TweetId,
                       BookmarkCount = x.BookmarkCount,
                       ConversationId = x.ConversationId,
                       CreatedAt = x.CreatedAt,
                       FavoriteCount = x.FavoriteCount,
                       FullText = x.FullText,
                       InReplyToScreenName = x.InReplyToScreenName,
                       InReplyToStatusId = x.InReplyToStatusId,
                       InReplyToUserId = x.InReplyToUserId,
                       IsQuoteStatus = x.IsQuoteStatus,
                       Lang = x.Lang,
                       QuoteCount = x.QuoteCount,
                       ReplyCount = x.ReplyCount,
                       RetweetCount = x.RetweetCount,
                       UserId = x.UserId,
                       UserName = x.UserName,
                       UserScreenName = x.UserScreenName,
                       ViewsCount = x.ViewsCount
                   }).Skip((pageNumber - 1) * pageSize).Take(pageSize)
                );

            var hashTags = await _tweetHashTagRepository.GetListAsync(x => pr.Items.Select(x => x.TweetId).Contains(x.TweetId));
            var urls = await _tweetUrlRepository.GetListAsync(x => pr.Items.Select(x => x.TweetId).Contains(x.TweetId));
            var symbols = await _tweetSymbolRepository.GetListAsync(x => pr.Items.Select(x => x.TweetId).Contains(x.TweetId));
            var mentions = await _tweetUserMentionRepository.GetListAsync(x => pr.Items.Select(x => x.TweetId).Contains(x.TweetId));
            var medias = await _tweetMediaRepository.GetListAsync(x => pr.Items.Select(x => x.TweetId).Contains(x.TweetId));

            foreach (var item in pr.Items)
            {
                var itemTags = hashTags.Where(x => x.TweetId == item.TweetId);
                if (itemTags.IsNotEmpty())
                {
                    item.HashTags = itemTags.Select(x => x.Text).Distinct().ToList();
                }

                var itemUrls = urls.Where(x => x.TweetId == item.TweetId);
                if (itemUrls.IsNotEmpty())
                {
                    item.Urls = itemUrls.Select(x => x.Url).Distinct().ToList();
                }

                var itemSymbols = symbols.Where(x => x.TweetId == item.TweetId);
                if (itemSymbols.IsNotEmpty())
                {
                    item.Symbols = itemSymbols.Select(x => x.SymbolText).Distinct().ToList();
                }

                var itemMentions = mentions.Where(x => x.TweetId == item.TweetId);
                if (itemMentions.IsNotEmpty())
                {
                    item.UserMentions = itemMentions.Select(x => new TweetDto.UserMentionDto()
                    {
                        Name = x.Name,
                        ScreenName = x.ScreenName,
                        UserId = x.UserId,
                    }).ToList();
                }

                var itemMedias = medias.Where(x => x.TweetId == item.TweetId);
                if (itemMedias.IsNotEmpty())
                {
                    item.Medias = itemMedias.Select(x => new TweetDto.MediaDto()
                    {
                        DisplayUrl = x.DisplayUrl,
                        ExpandedUrl = x.ExpandedUrl,
                        MediaId = x.MediaId,
                        MediaUrlHttps = x.MediaUrlHttps,
                        Type = x.Type,
                        Url = x.Url
                    }).ToList();
                }
            }

            return pr;
        }

        public async Task<string> UpdateUserStatusAsync([Required] List<string> userIds, [Required] string status)
        {
            var userStatuses = await _tweetUserStatusRepository.GetListAsync(x => userIds.Contains(x.UserId));
            foreach (var userId in userIds)
            {
                var userStatus = userStatuses.FirstOrDefault(x => x.UserId == userId);
                if (userStatus == null)
                {
                    userStatus = await _tweetUserStatusRepository.InsertAsync(new TwitterUserStatusEntity()
                    {
                        UserId = userId,
                        Status = status
                    });
                }
                else
                {
                    userStatus.Status = status;
                    await _tweetUserStatusRepository.UpdateAsync(userStatus);
                }
            }

            return "success";
        }

        public async Task<string> UpdateUserTypeAsync([Required] List<string> userIds, [Required] string type)
        {
            var userTypes = await _tweetUserTypeRepository.GetListAsync(x => userIds.Contains(x.UserId));
            foreach (var userId in userIds)
            {
                var userType = userTypes.FirstOrDefault(x => x.UserId == userId);
                if (userType == null)
                {
                    userType = await _tweetUserTypeRepository.InsertAsync(new TwitterUserTypeEntity()
                    {
                        UserId = userId,
                        Type = type
                    });
                }
                else
                {
                    userType.Type = type;
                    await _tweetUserTypeRepository.UpdateAsync(userType);
                }
            }

            return "success";
        }

        [Obsolete("Hàm cũ dùng để tham khảo")]
        private async Task<PagingResult<TweetMentionDto>> GetMentionListAsync_bak(int pageNumber, int pageSize, string userStatus, string userType, string searchText, string ownerUserScreenName)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            var pr = await _twitterTweetMentionDapperRepository.GetMentionListAsync(pageNumber, pageSize, userStatus, userType, searchText, ownerUserScreenName);
            if (pr.TotalCount == 0)
            {
                return pr;
            }

            var tags = await _tweetHashTagRepository.GetListAsync(x => pr.Items.Select(x => x.LastestTweetId).Contains(x.TweetId));
            foreach (var item in pr.Items)
            {
                var itemTags = tags.Where(x => x.TweetId == item.LastestTweetId);
                item.HashTags = itemTags.Select(x => x.Text).Distinct().ToList();

                if (item.UserStatus.IsEmpty())
                {
                    item.UserStatus = "New";
                }
            }

            return pr;
        }
    }
}
