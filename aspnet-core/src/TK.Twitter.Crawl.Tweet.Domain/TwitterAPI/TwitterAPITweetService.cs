using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.TwitterAPI.Dto;
using TK.TwitterAccount.Domain;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace TK.Twitter.Crawl.TwitterAPI
{
    public class TwitterAPITweetService : DomainService
    {
        private readonly TwitterAPIAuthService _twitterAuthService;
        private readonly ITwitterAccountRepository _twitterAccountRepository;
        private readonly IRepository<TwitterCrawlAccountEntity, long> _twitterCrawlAccountRepository;

        public TwitterAPITweetService(
            HttpClient httpClient,
            TwitterAPIAuthService twitterAuthService,
            ITwitterAccountRepository twitterAccountRepository,
            IRepository<TwitterCrawlAccountEntity, long> twitterCrawlAccountRepository)
        {
            HttpClient = httpClient;
            _twitterAuthService = twitterAuthService;
            _twitterAccountRepository = twitterAccountRepository;
            _twitterCrawlAccountRepository = twitterCrawlAccountRepository;
            HttpClient.DefaultRequestHeaders.Add("authorization", "Bearer AAAAAAAAAAAAAAAAAAAAANRILgAAAAAAnNwIzUejRCOuH5E6I8xnZz4puTs%3D1Zv7ttfk8LF81IUq16cHjhLTvJu4FA33AGWWjCpTnA");
            HttpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36");
            HttpClient.DefaultRequestHeaders.Add("x-twitter-active-user", "yes");
            HttpClient.DefaultRequestHeaders.Add("x-twitter-auth-type", "OAuth2Session");
            HttpClient.DefaultRequestHeaders.Add("x-twitter-client-language", "en");
        }

        public HttpClient HttpClient { get; }

        public async Task<TwitterAPIGetTweetResponse> GetTweetAsync(string userId, string crawlAccountId, bool requiredLogin = false, string cursor = null)
        {
            var crawlAccountInfo = await _twitterCrawlAccountRepository.FirstOrDefaultAsync(x => x.AccountId == crawlAccountId);
            if (crawlAccountInfo == null || requiredLogin)
            {
                crawlAccountInfo = await _twitterAuthService.CheckLogin(crawlAccountId);
            }

            string url = "https://twitter.com/i/api/graphql/XicnWRbyQ3WgVY__VataBQ/UserTweets";

            var varibles = new
            {
                userId = userId,
                count = 20,
                includePromotedContent = true,
                withQuickPromoteEligibilityTweetFields = true,
                withVoice = true,
                withV2Timeline = true,
                cursor
            };

            var features = new
            {
                rweb_lists_timeline_redesign_enabled = true,
                responsive_web_graphql_exclude_directive_enabled = true,
                verified_phone_label_enabled = false,
                creator_subscriptions_tweet_preview_api_enabled = true,
                responsive_web_graphql_timeline_navigation_enabled = true,
                responsive_web_graphql_skip_user_profile_image_extensions_enabled = false,
                tweetypie_unmention_optimization_enabled = true,
                responsive_web_edit_tweet_api_enabled = true,
                graphql_is_translatable_rweb_tweet_is_translatable_enabled = true,
                view_counts_everywhere_api_enabled = true,
                longform_notetweets_consumption_enabled = true,
                responsive_web_twitter_article_tweet_consumption_enabled = false,
                tweet_awards_web_tipping_enabled = false,
                freedom_of_speech_not_reach_fetch_enabled = true,
                standardized_nudges_misinfo = true,
                tweet_with_visibility_results_prefer_gql_limited_actions_policy_enabled = true,
                longform_notetweets_rich_text_read_enabled = true,
                longform_notetweets_inline_media_enabled = true,
                responsive_web_media_download_video_enabled = false,
                responsive_web_enhance_cards_enabled = false
            };

            url += "?variables=" + WebUtility.UrlEncode(JsonHelper.Stringify(varibles));
            url += "&features=" + WebUtility.UrlEncode(JsonHelper.Stringify(features));

            return await SendAsync(url, crawlAccountInfo.GuestToken, crawlAccountInfo.CookieCtZeroValue, crawlAccountInfo.Cookie);
        }

        private async Task<TwitterAPIGetTweetResponse> SendAsync(string url, string guestToken, string ctZeroValue, string cookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-guest-token", guestToken);
            request.Headers.Add("x-csrf-token", ctZeroValue);
            request.Headers.Add("Cookie", cookie);

            var response = await HttpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            int rateLimit = 0;
            int rateLimitRemaining = 0;
            int rateLimitResetAtTimeStamp = 0;
            if (response.Headers.TryGetValues("x-rate-limit-limit", out var eRateLimit))
            {
                if (eRateLimit.IsNotEmpty())
                {
                    rateLimit = int.Parse(eRateLimit.FirstOrDefault());
                }
            }

            if (response.Headers.TryGetValues("x-rate-limit-remaining", out var eRemaining))
            {
                if (eRemaining.IsNotEmpty())
                {
                    rateLimitRemaining = int.Parse(eRemaining.FirstOrDefault());
                }
            }

            if (response.Headers.TryGetValues("x-rate-limit-reset", out var eRateLimitReset))
            {
                if (eRateLimitReset.IsNotEmpty())
                {
                    rateLimitResetAtTimeStamp = int.Parse(eRateLimitReset.FirstOrDefault());
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var r = new TwitterAPIGetTweetResponse();
                    r.TooManyRequest = true;
                    r.RateLimit = rateLimit;
                    r.RateLimitRemaining = rateLimitRemaining;
                    r.RateLimitResetAtTimeStamp = rateLimitResetAtTimeStamp;
                    return r;
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    if (content.IsEmpty())
                    {
                        throw new BusinessException(CrawlDomainErrorCodes.TwitterAuthorizationError, "Can not get anything");
                    }

                    var tmp = JsonHelper.Parse<TwitterAPIGetTweetResponse>(content);
                    if (tmp.Errors.IsNotEmpty())
                    {
                        var authoriztionError = tmp.Errors.FirstOrDefault(x => x.Code == 37);
                        if (authoriztionError != null)
                        {
                            throw new BusinessException(CrawlDomainErrorCodes.TwitterAuthorizationError, authoriztionError.Message);
                        }

                        var requireMatchCrsfAndHeaderError = tmp.Errors.FirstOrDefault(x => x.Code == 353);
                        if (requireMatchCrsfAndHeaderError != null)
                        {
                            throw new BusinessException(CrawlDomainErrorCodes.TwitterAuthorizationError, requireMatchCrsfAndHeaderError.Message);
                        }
                    }
                }
                else
                {
                    throw new BusinessException(CrawlDomainErrorCodes.TwitterUnexpectedError, content);
                }
            }

            var result = JsonHelper.Parse<TwitterAPIGetTweetResponse>(content);
            result.RateLimit = rateLimit;
            result.RateLimitRemaining = rateLimitRemaining;
            result.RateLimitResetAtTimeStamp = rateLimitResetAtTimeStamp;
            result.JsonText = content;

            return result;
        }
    }
}
