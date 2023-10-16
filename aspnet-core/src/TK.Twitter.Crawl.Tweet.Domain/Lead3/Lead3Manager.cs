using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Entity.Dapper;
using TK.Twitter.Crawl.Repository;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using static TK.Twitter.Crawl.CrawlConsts;

namespace TK.Twitter.Crawl.Tweet
{
    public class Lead3Manager : DomainService
    {
        private readonly IRepository<TwitterTweetEntity, long> _tweetRepository;
        private readonly IRepository<TwitterUserTypeEntity, long> _tweetUserTypeRepository;
        private readonly IRepository<TwitterUserStatusEntity, long> _tweetUserStatusRepository;
        private readonly IRepository<TwitterTweetUserMentionEntity, long> _tweetUserMentionRepository;
        private readonly IRepository<TwitterTweetUrlEntity, long> _tweetUrlRepository;
        private readonly IRepository<TwitterTweetSymbolEntity, long> _tweetSymbolRepository;
        private readonly IRepository<TwitterTweetMediaEntity, long> _tweetMediaRepository;
        private readonly IRepository<TwitterTweetHashTagEntity, long> _tweetHashTagRepository;
        private readonly IRepository<TwitterUserSignalEntity, long> _twitterUserSignalRepository;
        private readonly IRepository<AirTableLeadRecordMappingEntity, long> _airTableLeadRecordMappingRepository;
        private readonly IRepository<AirTableManualSourceEntity, long> _airTableManualSourceRepository;
        private readonly ITwitterTweetMentionDapperRepository _twitterTweetMentionDapperRepository;

        public Lead3Manager(
            IRepository<TwitterTweetEntity, long> tweetRepository,
            IRepository<TwitterUserTypeEntity, long> tweetUserTypeRepository,
            IRepository<TwitterUserStatusEntity, long> tweetUserStatusRepository,
            IRepository<TwitterTweetUserMentionEntity, long> tweetUserMentionRepository,
            IRepository<TwitterTweetUrlEntity, long> tweetUrlRepository,
            IRepository<TwitterTweetSymbolEntity, long> tweetSymbolRepository,
            IRepository<TwitterTweetMediaEntity, long> tweetMediaRepository,
            IRepository<TwitterTweetHashTagEntity, long> tweetHashTagRepository,
            IRepository<TwitterUserSignalEntity, long> twitterUserSignalRepository,
            IRepository<AirTableLeadRecordMappingEntity, long> airTableLeadRecordMappingRepository,
            IRepository<AirTableManualSourceEntity, long> airTableManualSourceRepository,
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
            _twitterUserSignalRepository = twitterUserSignalRepository;
            _airTableLeadRecordMappingRepository = airTableLeadRecordMappingRepository;
            _airTableManualSourceRepository = airTableManualSourceRepository;
            _twitterTweetMentionDapperRepository = twitterTweetMentionDapperRepository;
        }

        public async Task<PagingResult<TweetMentionDto>> GetLeadsAsync(
            int pageNumber,
            int pageSize,
            string userStatus = null,
            string userType = null,
            string searchText = null,
            string ownerUserScreenName = null,
            string signal = null)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            var query = await GetLeadQueryableAsync(userStatus, userType, searchText, ownerUserScreenName, signal);

            var pr = new PagingResult<TweetMentionDto>();
            pr.TotalCount = await AsyncExecuter.CountAsync(query);
            if (pr.TotalCount == 0)
            {
                return pr;
            }

            query = query.OrderByDescending(x => x.LastestSponsoredDate);
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            pr.Items = await AsyncExecuter.ToListAsync(query);

            await PrepareLeadDtoAsync(pr.Items);

            return pr;
        }

        public async Task<IQueryable<TweetMentionDto>> GetLeadQueryableAsync(
           string userStatus = null,
           string userType = null,
           string searchText = null,
           string ownerUserScreenName = null,
           string signal = null)
        {
            var mentionQuery = await _tweetUserMentionRepository.GetQueryableAsync();
            var hashTagQuery = await _tweetHashTagRepository.GetQueryableAsync();
            var tweetQuery = await _tweetRepository.GetQueryableAsync();
            var signalQuery = await _twitterUserSignalRepository.GetQueryableAsync();

            var mentionWithFilterQuery = (
                                          from mention in mentionQuery
                                          where signalQuery.Any(x => x.TweetId == mention.TweetId && x.UserId == mention.UserId)
                                          select new
                                          {
                                              mention,
                                          }

                                      ).GroupBy(x => x.mention.UserId).Select(mt => new
                                      {
                                          UserId = mt.Key,
                                          MaxTweetCreatedAt = mt.Max(x => x.mention.TweetCreatedAt),
                                      });

            var tweetWithMentionCountQuery = from tweet in tweetQuery
                                             join mention in mentionQuery.Where(mt => signalQuery.Any(s => s.UserId == mt.UserId && s.TweetId == mt.TweetId)).GroupBy(x => x.TweetId).Select(x => new { TweetId = x.Key, MentionCount = x.Count() }) on tweet.TweetId equals mention.TweetId
                                             select new
                                             {
                                                 tweet,
                                                 mention.MentionCount
                                             };

            var query = from mention_main in mentionQuery
                        join mention_filter in mentionWithFilterQuery on mention_main.TweetCreatedAt equals mention_filter.MaxTweetCreatedAt

                        //join user_type_origin in await _tweetUserTypeRepository.GetQueryableAsync() on mention_main.UserId equals user_type_origin.UserId
                        //into user_type_temp
                        //from user_type in user_type_temp.DefaultIfEmpty()

                        //join user_status_origin in await _tweetUserStatusRepository.GetQueryableAsync() on mention_main.UserId equals user_status_origin.UserId
                        //into user_status_temp
                        //from user_status in user_status_temp.DefaultIfEmpty()

                        where mention_main.UserId == mention_filter.UserId

                        select new
                        {
                            mention_main,
                            //user_status.Status,
                            //user_type.Type,
                        };

            var userStatusQuery = await _tweetUserStatusRepository.GetQueryableAsync();
            var userTypeQuery = await _tweetUserTypeRepository.GetQueryableAsync();

            query = query.WhereIf(userStatus.IsNotEmpty(), x => userStatusQuery.Any(us => us.UserId == x.mention_main.UserId && us.Status == userStatus));
            query = query.WhereIf(userType.IsNotEmpty(), x => userTypeQuery.Any(ut => ut.UserId == x.mention_main.UserId && ut.Type == userType));
            query = query.WhereIf(ownerUserScreenName.IsNotEmpty(), x => tweetWithMentionCountQuery.Any(x => x.tweet.UserScreenNameNormalize == ownerUserScreenName));
            query = query.WhereIf(searchText.IsNotEmpty(), x => x.mention_main.NormalizeScreenName.Contains(searchText.ToLower())
                                                                || tweetWithMentionCountQuery.Any(x => x.tweet.FullText.Contains(searchText.ToLower())));
            query = query.WhereIf(signal.IsNotEmpty(), x => signalQuery.Any(s => s.Signal == signal && x.mention_main.UserId == s.UserId));

            return query.Select(q => new TweetMentionDto()
            {
                UserId = q.mention_main.UserId,
                LastestTweetId = q.mention_main.TweetId,
                UserName = q.mention_main.Name,
                UserScreenName = q.mention_main.ScreenName,
                //UserType = q.Type,
                //UserStatus = q.Status,
                LastestSponsoredDate = q.mention_main.TweetCreatedAt
            });
        }

        public async Task<List<TweetMentionDto>> PrepareLeadDtoAsync(List<TweetMentionDto> items)
        {
            var mentionQuery = await _tweetUserMentionRepository.GetQueryableAsync();
            var tweetQuery = await _tweetRepository.GetQueryableAsync();
            var signalQuery = await _twitterUserSignalRepository.GetQueryableAsync();

            var tweetWithMentionCountQuery = from tweet in tweetQuery
                                             join mention in mentionQuery.Where(mt => signalQuery.Any(s => s.UserId == mt.UserId && s.TweetId == mt.TweetId))
                                                                         .GroupBy(x => x.TweetId).Select(x => new { TweetId = x.Key, MentionCount = x.Count() }) on tweet.TweetId equals mention.TweetId
                                             select new
                                             {
                                                 tweet,
                                                 mention.MentionCount
                                             };

            var tags = await _tweetHashTagRepository.GetListAsync(x => items.Select(x => x.LastestTweetId).Contains(x.TweetId));
            var lastestTweets = await AsyncExecuter.ToListAsync(tweetWithMentionCountQuery.Where(x => items.Select(x => x.LastestTweetId).Contains(x.tweet.TweetId)));
            var signals = await _twitterUserSignalRepository.GetListAsync(x => items.Select(x => x.UserId).Contains(x.UserId));
            var userStatuses = await _tweetUserStatusRepository.GetListAsync(x => items.Select(x => x.UserId).Contains(x.UserId));
            var userTypes = await _tweetUserTypeRepository.GetListAsync(x => items.Select(x => x.UserId).Contains(x.UserId));

            var anotherSignals = await GetSignalDescription(items.Select(x => x.UserId).Distinct());

            foreach (var item in items)
            {
                var itemTags = tags.Where(x => x.TweetId == item.LastestTweetId);
                item.HashTags = itemTags.Select(x => x.Text).Distinct().ToList();

                var tweet = lastestTweets.FirstOrDefault(x => x.tweet.TweetId == item.LastestTweetId);
                item.LastestSponsoredDate = tweet?.tweet.CreatedAt;
                item.TweetDescription = tweet?.tweet.FullText;
                item.TweetOwnerUserId = tweet?.tweet.UserId;
                item.MediaMentioned = tweet?.tweet.UserScreenNameNormalize;
                item.DuplicateUrlCount = tweet?.MentionCount;

                item.LastestSponsoredTweetUrl = "https://twitter.com/_/status/" + item.LastestTweetId; // url k cần quan tâm đên username nên thay bằng _

                // Nếu signal đến từ user thông báo listing CGK/CMK thì xử lý lại Signal From, Description, SignalUrl
                if (item.TweetOwnerUserId == CrawlConsts.TwitterUser.BOT_NEW_LISTING_CMC_CGK_USER_ID)
                {
                    if (item.HashTags.Any(ht => ht.EqualsIgnoreCase("coingecko")))
                    {
                        item.MediaMentioned = "Coingecko";
                        item.LastestSponsoredTweetUrl = "https://www.coingecko.com/en/new-cryptocurrencies";
                        item.TweetDescription = "Just listed in Coingecko";
                        item.TweetOwnerUserId = null;
                    }
                    else if (item.HashTags.Any(ht => ht.EqualsIgnoreCase("coinmarketcap")))
                    {
                        item.MediaMentioned = "Coinmarketcap";
                        item.LastestSponsoredTweetUrl = "https://coinmarketcap.com/new/";
                        item.TweetDescription = "Just listed in Coinmarketcap";
                        item.TweetOwnerUserId = null;
                    }
                }

                var itemSignals = signals.Where(x => x.UserId == item.UserId);
                item.NumberOfSponsoredTweets = itemSignals.Count();
                item.Signals = itemSignals.Select(x => x.Signal).Distinct().ToList();

                var userType = userTypes.FirstOrDefault(x => x.UserId == item.UserId);
                item.UserType = userType?.Type;

                var userStatus = userStatuses.FirstOrDefault(x => x.UserId == item.UserId);
                item.UserStatus = userStatus?.Status;

                if (anotherSignals.TryGetValue(item.UserId, out string otherSignal))
                {
                    item.SignalDescription = otherSignal;
                }
            }

            return items;
        }

        public async Task<List<TweetMentionDto>> GetLeadsAsync(List<string> userIds = null, bool notExistInAirTable = false)
        {
            var query = await GetLeadQueryableAsync();

            var airTableMappingQuery = await _airTableLeadRecordMappingRepository.GetQueryableAsync();
            query = query.WhereIf(notExistInAirTable, x => !airTableMappingQuery.Any(a => a.ProjectUserId == x.UserId));
            query = query.WhereIf(userIds.IsNotEmpty(), x => userIds.Contains(x.UserId));

            var items = await AsyncExecuter.ToListAsync(query);
            if (items.IsEmpty())
            {
                return items;
            }

            await PrepareLeadDtoAsync(items);

            return items;
        }

        public async Task<Dictionary<string, string>> GetSignalDescription(IEnumerable<string> userIds)
        {
            var dict = new Dictionary<string, string>();

            var mentionQuery = await _tweetUserMentionRepository.GetQueryableAsync();
            var tweetQuery = await _tweetRepository.GetQueryableAsync();
            var signalQuery = await _twitterUserSignalRepository.GetQueryableAsync();
            var manualSourceQuery = await _airTableManualSourceRepository.GetQueryableAsync();

            var anotherSignalQuery = from tweet in tweetQuery
                                     join signal in signalQuery on tweet.TweetId equals signal.TweetId
                                     where userIds.Contains(signal.UserId)
                                     && mentionQuery.Any(mention => mention.UserId == signal.UserId && mention.TweetId == tweet.TweetId)
                                     select new { UserId = signal.UserId, Owner = tweet.UserScreenName, OwnerId = tweet.UserId, Signal = signal.Signal };

            var anotherSignals = await AsyncExecuter.ToListAsync(anotherSignalQuery);

            var manualSourceSignals = await AsyncExecuter.ToListAsync(

                from s in signalQuery
                join ms in manualSourceQuery on s.AirTableRecordId equals ms.RecordId
                where s.Source == CrawlConsts.Signal.Source.AIR_TABLE_MANUAL_SOURCE && userIds.Contains(s.UserId)
                select new { UserId = s.UserId, Owner = ms.LastestSignalFrom, OwnerId = ms.UserId, Signal = s.Signal }
                );

            var signals = anotherSignals.Union(manualSourceSignals).ToList();

            foreach (var userId in userIds)
            {
                var anotherSignal = signals.Where(x => x.UserId == userId).ToList();
                if (anotherSignal.IsNotEmpty())
                {
                    var sb = new StringBuilder();
                    var groupBy = anotherSignal.GroupBy(x => new { x.UserId, x.Signal, x.Owner, x.OwnerId }).Select(x => new { x.Key, Count = x.Count() });
                    foreach (var gb in groupBy)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(Environment.NewLine);
                        }

                        var signal = CrawlConsts.Signal.GetName(gb.Key.Signal);
                        if (signal.IsEmpty())
                        {
                            continue;
                        }

                        if (gb.Key.OwnerId == CrawlConsts.TwitterUser.BOT_NEW_LISTING_CMC_CGK_USER_ID) // Đây là tài khoản Crawl signal new listing CGK/CMC
                        {
                            sb.Append($"● {signal}");
                        }
                        else
                        {
                            sb.Append($"● {signal} @ {gb.Key.Owner}");
                            if (gb.Count > 1)
                            {
                                sb.Append($" {gb.Count} times");
                            }
                        }
                    }
                    dict.Add(userId, sb.ToString());
                }
            }

            return dict;
        }
    }
}
