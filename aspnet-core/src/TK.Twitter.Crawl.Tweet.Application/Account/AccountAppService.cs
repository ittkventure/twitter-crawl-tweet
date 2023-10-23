using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Account;
using Volo.Abp.Caching;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Volo.Abp.Emailing;
using TK.Twitter.Crawl.Tweet.Payment.Dto.CacheItem;
using System.ComponentModel.DataAnnotations;
using TK.Twitter.Crawl.Tweet.User;

namespace TK.Twitter.Crawl.Tweet.Payment
{
    public class AccountAppService : CrawlAppService
    {
        private readonly IAccountAppService _accountAppService;
        private readonly UserPlanManager _userPlanManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IDistributedCache<AccountVerifyTokenCacheItem, string> _verifyTokenCache;

        public AccountAppService(
            IAccountAppService accountAppService,
            UserPlanManager userPlanManager,
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            IRepository<IdentityUser, Guid> userRepository,
            IOptions<IdentityOptions> identityOptions,
            IDistributedEventBus distributedEventBus,
            IDistributedCache<AccountVerifyTokenCacheItem, string> verifyTokenCache)
        {
            _accountAppService = accountAppService;
            _userPlanManager = userPlanManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _userRepository = userRepository;
            _identityOptions = identityOptions;
            _distributedEventBus = distributedEventBus;
            _verifyTokenCache = verifyTokenCache;
        }

        /// <summary>
        /// Verify email
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        [Authorize]
        public async Task<bool> VerifyEmailAsync()
        {
            CheckLogin();

            var user = await _userManager.FindByIdAsync(CurrentUser.Id.Value.ToString());
            if (user == null)
            {
                throw new BusinessException(CrawlDomainErrorCodes.UserNotFound, "User not found");
            }

            if (user.Email.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.UserEmailNotProvided, "User's email is invalid");
            }

            if (user.EmailConfirmed)
            {
                throw new BusinessException(CrawlDomainErrorCodes.UserEmailConfirmed, "User verified email");
            }

            var token = await GenerateEmailConfirmationToken(user);
            await _emailSender.SendAsync(user.Email,
                                         "[lead3.io] Please verify your email",
                                          @$"<p>Hi {(user.Name.IsEmpty() ? user.UserName : user.Name)},</p>
<p>Welcome to <a href=""https://lead3.io"">AlphaQuest.io</a>! This email is sent directly to you by the Official lead3.io Team.</p>
<p>Please click the button below to verify your email address.</p>
<p>If you did not sign up to lead3.io, please ignore this email or contact us at contact@lead3.io</p>
<p>Thanks,</p>
<p>lead3.io team</p>
</br>
<p><a href=""https://lead3.io/user/verify?vr={token}"">VERIFY EMAIL NOW</a></p>");

            return true;
        }

        /// <summary>
        /// Thực hiện xác nhận email
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<IdentityResult> ConfirmEmailAsync(AccountConfirmEmailInputDto input)
        {
            var cacheKey = GetVerifyTokenCacheKey(input.Token);
            var cache = await _verifyTokenCache.GetAsync(cacheKey);
            if (cache == null)
            {
                throw new BusinessException(CrawlDomainErrorCodes.UserConfirmEmailTokenInvalid);
            }

            var user = await _userManager.FindByIdAsync(cache.UserId.ToString());
            if (user == null)
            {
                throw new BusinessException(CrawlDomainErrorCodes.UserNotFound, "User not found");
            }

            if (user.EmailConfirmed)
            {
                throw new BusinessException(CrawlDomainErrorCodes.UserEmailAlreadyVerified, "User verified email");
            }

            if (user.Email.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.UserEmailNotProvided, "User's email is invalid");
            }

            var result = await _userManager.ConfirmEmailAsync(user, input.Token);
            if (!result.Succeeded)
            {
                throw new BusinessException(CrawlDomainErrorCodes.UserConfirmEmailTokenInvalid, "Invalid email verification code");
            }

            return result;
        }

        /// <summary>
        /// Tạo yêu cầu reset password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="callBackUrl"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<bool> ResetPasswordAsync(string email)
        {
            if (email.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.InputModelNotValidated, "Email is empty");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new BusinessException(CrawlDomainErrorCodes.UserNotFound, "User not found");
            }

            await _identityOptions.SetAsync();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _emailSender.SendAsync(email,
                "Reset password",
                $@"<h3>Password reset</h3>
                <p>We received an account recovery request! If you initiated this request, click the following link to reset your password.</p>
                <div>
                    <a href=""https://lead3.io/user/change-password?email={email}&token={token}"">Reset my password</a>
                </div>");

            return true;
        }

        /// <summary>
        /// Xác nhận reset password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <param name="password"></param>
        /// <param name="confirmPassword"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<IdentityResult> ConfirmResetPasswordAsync(AccountConfirmResetPasswordInputDto input)
        {
            if (input.Email.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.InputModelNotValidated, "Email is invalid");
            }

            if (input.Token.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.InputModelNotValidated, "Token is invalid");
            }

            if (input.Password.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.InputModelNotValidated, "Password is invalid");
            }

            if (input.ConfirmPassword.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.InputModelNotValidated, "Confirm password is invalid");
            }

            if (input.Password != input.ConfirmPassword)
            {
                throw new BusinessException(CrawlDomainErrorCodes.InputModelNotValidated, "Password and confirm password do not match");
            }

            var user = await _userManager.FindByEmailAsync(input.Email);
            if (user == null)
            {
                throw new BusinessException(CrawlDomainErrorCodes.UserNotFound, "User not found");
            }

            var result = await _userManager.ResetPasswordAsync(user, input.Token, input.ConfirmPassword);
            if (!result.Succeeded)
            {
                return result;
            }

            // confirm 
            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmEmailResult = await _userManager.ConfirmEmailAsync(user, token);
            }

            return result;
        }

        [Authorize]
        public async Task<AccountDetailDto> GetDetailAsync()
        {
            CheckLogin();

            var query = from u in (await _userRepository.GetQueryableAsync()).Where(x => x.Id == CurrentUser.Id.Value)
                        select new AccountDetailDto()
                        {
                            Username = u.UserName,
                            Name = u.Name,
                            Email = u.Email,
                            CreatedAt = u.CreationTime,
                            ConfirmedEmail = u.EmailConfirmed,
                        };

            var user = await _userRepository.AsyncExecuter.FirstOrDefaultAsync(query);
            if (user == null)
            {
                throw new BusinessException(CrawlDomainErrorCodes.NotFound, "User not found");
            }

            var currentPlan = await _userPlanManager.GetCurrentPlan(CurrentUser.Id.Value);
            var (hasPremium, expiredAt) = await _userPlanManager.CheckPaidPlan(CurrentUser.Email);

            if (hasPremium)
            {
                user.CurrentPlanKey = currentPlan.PlanKey;
                user.PlanExpiredAt = expiredAt;
            }
            else
            {
                user.CurrentPlanKey = CrawlConsts.Payment.FREE;
            }

            return user;
        }

        [Authorize]
        public async Task<IdentityResult> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            CheckLogin();

            if (currentPassword.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.InputModelNotValidated, "Current Password is invalid");
            }

            if (newPassword.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.InputModelNotValidated, "New password is invalid");
            }

            if (currentPassword == newPassword)
            {
                throw new BusinessException(CrawlDomainErrorCodes.InputModelNotValidated, "Current Password and New Password is the same");
            }

            var user = await _userManager.FindByEmailAsync(CurrentUser.Email);
            if (user == null)
            {
                throw new BusinessException(CrawlDomainErrorCodes.UserNotFound, "User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result;
        }

        private static string GetVerifyTokenCacheKey(string token)
        {
            return $"__verify_token_{token}__";
        }

        private async Task<string> GenerateEmailConfirmationToken(IdentityUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var cacheKey = GetVerifyTokenCacheKey(token);
            await _verifyTokenCache.RemoveAsync(cacheKey);
            await _verifyTokenCache.SetAsync(
                cacheKey,
                new AccountVerifyTokenCacheItem()
                {
                    UserId = user.Id
                },
                new DistributedCacheEntryOptions()
                {
                    AbsoluteExpiration = Clock.Now.AddDays(1),
                });

            return token;
        }

    }



    public class AccountConfirmResetPasswordInputDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

    }

    public class AccountConfirmEmailInputDto
    {
        [Required]
        public string Token { get; set; }
    }

    public class AccountDetailDto
    {
        public string Name { get; set; }

        public string Username { get; set; }

        /// <summary>
        /// Plan hiện tại
        /// </summary>
        public string CurrentPlanKey { get; set; }

        public DateTime? PlanExpiredAt { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string Email { get; set; }

        public bool ConfirmedEmail { get; set; }

    }
}
