using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using Volo.Abp.Domain.Repositories;

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

        public TweetAppService(
            IRepository<TwitterTweetEntity, long> tweetRepository,
            IRepository<TwitterUserTypeEntity, long> tweetUserTypeRepository,
            IRepository<TwitterUserStatusEntity, long> tweetUserStatusRepository,
            IRepository<TwitterTweetUserMentionEntity, long> tweetUserMentionRepository,
            IRepository<TwitterTweetUrlEntity, long> tweetUrlRepository,
            IRepository<TwitterTweetSymbolEntity, long> tweetSymbolRepository,
            IRepository<TwitterTweetMediaEntity, long> tweetMediaRepository,
            IRepository<TwitterTweetHashTagEntity, long> tweetHashTagRepository)
        {
            _tweetRepository = tweetRepository;
            _tweetUserTypeRepository = tweetUserTypeRepository;
            _tweetUserStatusRepository = tweetUserStatusRepository;
            _tweetUserMentionRepository = tweetUserMentionRepository;
            _tweetUrlRepository = tweetUrlRepository;
            _tweetSymbolRepository = tweetSymbolRepository;
            _tweetMediaRepository = tweetMediaRepository;
            _tweetHashTagRepository = tweetHashTagRepository;
        }

        public async Task<PagingResult<TweetMentionDto>> GetMentionListAsync(int pageNumber, int pageSize, string userStatus, string userType, string searchText)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            var mentionUserQuery = await _tweetUserMentionRepository.GetQueryableAsync();
            var hashTagQuery = await _tweetHashTagRepository.GetQueryableAsync();

            var ignoreScreenName = new List<string>()
            {
                "binance",
                "coinbase",
                "bnbchain",
                "epicgames",
                "bitfinex",
                "bitmartexchange",
            };

            var mentionUserIgnoreAmaWinnerQuery = from mt in mentionUserQuery
                                                  join tweet in await _tweetRepository.GetQueryableAsync() on mt.TweetId equals tweet.TweetId
                                                  where
                                                  !hashTagQuery.Any(x => x.TweetId == mt.TweetId && x.NormalizeText == "ama" && tweet.NormalizeFullText.Contains("winner")) // bỏ các tag ama nhưng description lại có chữ winner
                                                  && !ignoreScreenName.Contains(mt.ScreenName) // bỏ các tweet mention partner lớn để bú fame
                                                  && mt.UserId == tweet.UserId // bỏ các tweet tự mention chính nó
                                                  select mt;

            var lastestMentionUserQuery = from g in mentionUserIgnoreAmaWinnerQuery.GroupBy(x => x.UserId)
                                          select new
                                          {
                                              UserId = g.Key,
                                              CreationTime = g.Max(x => x.CreationTime),
                                              Count = g.Count()
                                          };

            lastestMentionUserQuery = lastestMentionUserQuery.Distinct();

            var mentionUserIdQuery = mentionUserQuery.OrderByDescending(x => x.CreationTime).GroupBy(x => x.UserId).Select(x => x.Key);

            var query = from mentionUser in mentionUserQuery
                        join lastMentionUser in lastestMentionUserQuery on mentionUser.UserId equals lastMentionUser.UserId
                        join userTypeQ in await _tweetUserTypeRepository.GetQueryableAsync() on mentionUser.UserId equals userTypeQ.UserId
                        into mentionUserIdUserTypeTemp
                        from ut in mentionUserIdUserTypeTemp.DefaultIfEmpty()
                        join userStatusQ in await _tweetUserStatusRepository.GetQueryableAsync() on ut.UserId equals userStatusQ.UserId
                        into mentionUserUserStatusTemp
                        from us in mentionUserUserStatusTemp.DefaultIfEmpty()
                        join tweet in await _tweetRepository.GetQueryableAsync() on mentionUser.TweetId equals tweet.TweetId
                        where mentionUser.CreationTime == lastMentionUser.CreationTime
                        select new TweetMentionDto()
                        {
                            UserId = mentionUser.UserId,
                            LastestTweetId = mentionUser.TweetId,
                            UserName = mentionUser.Name,
                            UserScreenName = mentionUser.ScreenName,
                            NormalizeUserScreenName = mentionUser.NormalizeScreenName,
                            NormalizeUserName = mentionUser.NormalizeName,
                            UserType = ut.Type,
                            UserStatus = us.Status,
                            LastestSponsoredDate = tweet.CreatedAt,
                            LastestSponsoredTweetUrl = "https://twitter.com/_/status/" + mentionUser.TweetId, // url k cần quan tâm đên username nên thay bằng _
                            TweetDescription = tweet.FullText,
                            NormalizeTweetDescription = tweet.NormalizeFullText,
                            TweetOwnerUserId = tweet.UserId,
                            MediaMentioned = tweet.UserScreenName,
                            NumberOfSponsoredTweets = lastMentionUser.Count
                        };

            query = query.WhereIf(userStatus.IsNotEmpty(), x => x.UserStatus == userStatus);
            query = query.WhereIf(userType.IsNotEmpty(), x => x.UserType == userType);
            query = query.WhereIf(searchText.IsNotEmpty(), x => x.NormalizeUserScreenName.Contains(searchText.ToLower())
                                                                || x.NormalizeUserName.Contains(searchText.ToLower())
                                                                || x.NormalizeTweetDescription.Contains(searchText.ToLower()));

            var pr = new PagingResult<TweetMentionDto>();
            pr.TotalCount = await AsyncExecuter.CountAsync(query);
            if (pr.TotalCount == 0)
            {
                return pr;
            }

            pr.Items = await AsyncExecuter.ToListAsync(query.Skip((pageNumber - 1) * pageSize).Take(pageSize));

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
                   }).Skip(pageNumber).Take(pageSize)
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

        public async Task<string> UpdateUserStatusAsync([Required] string userId, [Required] string status)
        {
            var userStatus = await _tweetUserStatusRepository.FirstOrDefaultAsync(x => x.UserId == userId);
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

            return "success";
        }

        public async Task<string> UpdateUserTypeAsync([Required] string userId, [Required] string type)
        {
            var userType = await _tweetUserTypeRepository.FirstOrDefaultAsync(x => x.UserId == userId);
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

            return "success";
        }
    }
}
