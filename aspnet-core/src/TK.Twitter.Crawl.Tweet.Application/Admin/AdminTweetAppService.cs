using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
using TK.Twitter.Crawl.Tweet.Admin.Dto;
using TK.Twitter.Crawl.Tweet.Payment;
using TK.Twitter.Crawl.Tweet.User;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace TK.Twitter.Crawl.Tweet.Admin
{
    public class AdminTweetAppService : CrawlAppService
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
        private readonly UserPlanManager _userPlanManager;
        private readonly PaddleAfterWebhookLogAddedHandler _paddleAfterWebhookLogAddedHandler;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly IRepository<UserPlanEntity, Guid> _userPlanRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminTweetAppService(
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
            Lead3Manager lead3Manager,
            UserPlanManager userPlanManager,
            PaddleAfterWebhookLogAddedHandler paddleAfterWebhookLogAddedHandler,
            IRepository<IdentityUser, Guid> userRepository,
            IRepository<UserPlanEntity, Guid> userPlanRepository,
            UserManager<IdentityUser> userManager)
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
            _userPlanManager = userPlanManager;
            _paddleAfterWebhookLogAddedHandler = paddleAfterWebhookLogAddedHandler;
            _userRepository = userRepository;
            _userPlanRepository = userPlanRepository;
            _userManager = userManager;
        }

        #region Lead3

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

        #endregion

        #region subscriptions

        public async Task<PagingResult<SubcriberDto>> GetSubscriberListAsync(int pageNumber, int pageSize, string searchText, string sortBy)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            var query = from up in await _userPlanRepository.GetQueryableAsync()
                        join u in await _userRepository.GetQueryableAsync() on up.UserId equals u.Id
                        select new SubcriberDto()
                        {
                            UserId = u.Id,
                            Email = u.Email,
                            Plan = up.PlanKey,
                            SubscribedEndDate = up.ExpiredAt,
                            CreationTime = u.CreationTime
                        };

            var pagingResult = new PagingResult<SubcriberDto>();
            query = query.WhereIf(searchText.IsNotEmpty(), x => x.Email.Contains(searchText));
            if (sortBy.IsEmpty())
            {
                query = query.OrderByDescending(x => x.CreationTime);
            }
            else
            {
                if (sortBy.EqualsIgnoreCase("modification_time_lastest"))
                {
                    query = query.OrderByDescending(x => x.CreationTime);
                }
                else if (sortBy.EqualsIgnoreCase("modification_time_oldest"))
                {
                    query = query.OrderBy(x => x.CreationTime);
                }
            }

            pagingResult.TotalCount = await AsyncExecuter.CountAsync(query);
            if (pagingResult.TotalCount == 0)
            {
                return pagingResult;
            }

            pagingResult.Items = await AsyncExecuter.ToListAsync(
                query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
                );

            foreach (var item in pagingResult.Items)
            {
                if (item.Plan == CrawlConsts.Payment.FREE)
                {
                    item.SubscribedEndDate = null;
                }
                else if (item.SubscribedEndDate <= Clock.Now)
                {
                    item.SubscribedEndDate = null;
                    item.Plan = CrawlConsts.Payment.FREE;
                }
            }

            return pagingResult;
        }

        public async Task<string> AddSubscriber([Required] string email, [Required] string planKey)
        {
            if (!CrawlConsts.Payment.CheckValid(planKey))
            {
                throw new BusinessException(CrawlDomainErrorCodes.PaymentInvalidPlan, "Invalid plan");
            }

            var userAlreadyExist = await _userRepository.AnyAsync(x => x.NormalizedEmail == email.ToUpper());
            if (userAlreadyExist)
            {
                throw new BusinessException(CrawlDomainErrorCodes.DuplicatedResource);
            }

            var user = await _paddleAfterWebhookLogAddedHandler.RegisterWithoutPasswordAsync(email, CrawlConsts.Payment.IsStandardPlan(planKey), autoSave: false);
            await _userPlanManager.UpgradeOrRenewalPlan(user.Id, planKey, paymentMethod: PaymentMethod.Unknown, "from_cms=true");
            return "success";
        }

        public async Task<string> SendEmailWelcome([Required] string email, [Required] string planKey)
        {
            // Gửi email thông báo đăng ký thành công kèm link để đổi mật khẩu
            var user = await _userManager.FindByEmailAsync(email);

            // Tạo reset password để khi user redirect lại trang sẽ đến trang đổi mật khẩu luôn
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _paddleAfterWebhookLogAddedHandler.SendEmailWelCome(email, CrawlConsts.Payment.IsStandardPlan(planKey), token);

            return "success";
        }

        #endregion
    }
}
