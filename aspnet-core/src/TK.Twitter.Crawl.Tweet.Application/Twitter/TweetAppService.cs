using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.Entity.Dapper;
using TK.Twitter.Crawl.Repository;
using TK.Twitter.Crawl.Tweet;
using TK.Twitter.Crawl.Tweet.Twitter.Dto;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace TK.Twitter.Crawl.Twitter
{
#if !DEBUG
    [Authorize]
    [RemoteService(IsMetadataEnabled = false)]
#endif
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
        private readonly IRepository<TwitterUserSignalEntity, long> _twitterUserSignalRepository;
        private readonly ITwitterTweetMentionDapperRepository _twitterTweetMentionDapperRepository;
        private readonly Lead3Manager _lead3Manager;

        public TweetAppService(
            IRepository<TwitterTweetEntity, long> tweetRepository,
            IRepository<TwitterUserTypeEntity, long> tweetUserTypeRepository,
            IRepository<TwitterUserStatusEntity, long> tweetUserStatusRepository,
            IRepository<TwitterTweetUserMentionEntity, long> tweetUserMentionRepository,
            IRepository<TwitterTweetUrlEntity, long> tweetUrlRepository,
            IRepository<TwitterTweetSymbolEntity, long> tweetSymbolRepository,
            IRepository<TwitterTweetMediaEntity, long> tweetMediaRepository,
            IRepository<TwitterTweetHashTagEntity, long> tweetHashTagRepository,
            IRepository<TwitterUserSignalEntity, long> twitterUserSignalRepository,
            ITwitterTweetMentionDapperRepository twitterTweetMentionDapperRepository,
            Lead3Manager lead3Manager)
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
            _twitterTweetMentionDapperRepository = twitterTweetMentionDapperRepository;
            _lead3Manager = lead3Manager;
        }

        public async Task<PagingResult<TweetMentionDto>> GetMentionListAsync(
            int pageNumber,
            int pageSize,
            string userStatus,
            string userType,
            string searchText,
            string ownerUserScreenName,
            string signal)
        {
            return await _lead3Manager.GetLeadsAsync(pageNumber, pageSize, userStatus, userType, searchText, ownerUserScreenName, signal);
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
            var signalQuery = await _twitterUserSignalRepository.GetQueryableAsync();
            var mentionQuery = await _tweetUserMentionRepository.GetQueryableAsync();
            var query = from tweet in await _tweetRepository.GetQueryableAsync()
                        where mentionQuery.Any(mention => mention.UserId == userId && mention.TweetId == tweet.TweetId)
                        && signalQuery.Any(x => x.TweetId == tweet.TweetId)
                        select tweet;

            query = query.WhereIf(searchText.IsNotEmpty(), x => x.NormalizeFullText.Contains(searchText));
            query = query.OrderByDescending(x => x.CreatedAt);

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
            var signals = await _twitterUserSignalRepository.GetListAsync(x => pr.Items.Select(x => x.TweetId).Contains(x.TweetId));

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

                var itemSignals = signals.Where(x => x.TweetId == item.TweetId);
                if (itemSignals.IsNotEmpty())
                {
                    item.Signals = itemSignals.Select(x => x.Signal).Distinct().ToList();
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
                        Status = status,
                        IsUserSuppliedValue = true
                    }, autoSave: true);
                }
                else
                {
                    userStatus.Status = status;
                    userStatus.IsUserSuppliedValue = true;
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
                        Type = type,
                        IsUserSuppliedValue = true
                    }, autoSave: true);
                }
                else
                {
                    userType.Type = type;
                    userType.IsUserSuppliedValue = true;
                    await _tweetUserTypeRepository.UpdateAsync(userType);
                }
            }

            return "success";
        }
    }
}
