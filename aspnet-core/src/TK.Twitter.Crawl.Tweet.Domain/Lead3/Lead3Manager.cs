﻿using System;
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
using static System.Collections.Specialized.BitVector32;
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
                        };

            query = query.WhereIf(userStatus.IsNotEmpty(), x => x.Status == userStatus);
            query = query.WhereIf(userType.IsNotEmpty(), x => x.Type == userType);
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
                UserType = q.Type,
                UserStatus = q.Status,
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

            var anotherSignalQuery = from tweet in tweetQuery
                                     join signal in signalQuery on tweet.TweetId equals signal.TweetId
                                     where items.Select(x => x.UserId).Contains(signal.UserId)
                                     && mentionQuery.Any(mention => mention.UserId == signal.UserId && mention.TweetId == tweet.TweetId)
                                     select new { signal.UserId, Owner = tweet.UserScreenName, signal.Signal };

            var anotherSignals = await AsyncExecuter.ToListAsync(anotherSignalQuery);

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

                var itemSignals = signals.Where(x => x.UserId == item.UserId);
                item.NumberOfSponsoredTweets = itemSignals.Count();
                item.Signals = itemSignals.Select(x => x.Signal).Distinct().ToList();

                var anotherSignal = anotherSignals.Where(x => x.UserId == item.UserId).ToList();
                if (anotherSignal.IsNotEmpty())
                {
                    var sb = new StringBuilder();
                    var groupBy = anotherSignal.GroupBy(x => new { x.UserId, x.Signal, x.Owner }).Select(x => new { x.Key, Count = x.Count() });
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

                        //Audit is completed @ solidproof
                        sb.Append($"{signal} @ {gb.Key.Owner}");
                        if (gb.Count > 1)
                        {
                            sb.Append($" {gb.Count} times");
                        }
                    }
                    item.SignalDescription = sb.ToString();
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
    }
}