using System.Net;
using TK.TwitterAccount.Domain.Dto;
using TK.TwitterAccount.Domain.Shared;
using Volo.Abp;

namespace TK.TwitterAccount.Domain.Clients
{
    public class TwitterUserClient : TwitterClient
    {
        private const string LOG_PREFIX = "[TwitterUserClient] ";

        private const string GetFollowingsUrl = "2/users/{0}/following?user.fields={1}&max_results={2}";
        private const string GetUserByUsernameUrl = "2/users/by/username/{0}";
        private const string GetListUserByUsernameUrl = "2/users/by?usernames=";
        private const string GetListUserByUserIdsUrl = "2/users?ids=";

        public TwitterUserClient(string bearerToken) : base()
        {
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        }

        /// <summary>
        /// Lấy danh sách người mà user đang follow
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="maxResult"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public async Task<TwitterGetFollowingResponse> GetFollowings(string userId, int maxResult = 100, params string[] fields)
        {
            if (userId.IsEmpty())
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (fields.IsEmpty())
            {
                throw new ArgumentNullException(nameof(fields));
            }

            if (maxResult <= 0)
            {
                maxResult = 100;
            }

            var url = string.Format(GetFollowingsUrl, userId, string.Join(",", fields), maxResult);
            var response = await Client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new BusinessException(TwitterAccountDomainErrorCodes.TooManyRequest, "An Error was due to the limit request of Twitter");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new BusinessException(TwitterAccountDomainErrorCodes.Unauthorized, "An Issued with Twitter Access token");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new BusinessException(TwitterAccountDomainErrorCodes.NotFound, $"User {userId} not found");
                }

                throw new BusinessException(TwitterAccountDomainErrorCodes.UnexpectedException, "An error occurred while retrieving the following information of the user " + userId);
            }

            var result = JsonHelper.Parse<TwitterGetFollowingResponse>(responseContent);
            if (result.TwitterErrors.IsNotEmpty())
            {
                var notFoundError = result.TwitterErrors.FirstOrDefault(x => x.ResourceType == "user" && x.Parameter == "id" && x.Title == "Not Found Error");
                if (notFoundError != null)
                {
                    throw new BusinessException(TwitterAccountDomainErrorCodes.NotFound, $"User {userId} not found");
                }

                throw new BusinessException(TwitterAccountDomainErrorCodes.UnexpectedException, "An error occurred while retrieving the following information of the user " + userId + " " + result.TwitterErrors.Select(x => $"{x.ResourceType} {x.ResourceId} {x.Title}").Distinct().JoinAsString(";"));
            }

            return result;
        }

        /// <summary>
        /// Lấy thông tin user theo username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<TwitterGetUserResponse> GetUserByUsernameAsync(string username, params string[] fields)
        {
            if (username.IsEmpty())
            {
                throw new ArgumentNullException(nameof(username));
            }

            var url = string.Format(string.Format(GetUserByUsernameUrl, username));
            if (fields.IsEmpty())
            {
                url += $"?user.fields=public_metrics";
            }
            else
            {
                url += $"?user.fields=" + fields.JoinAsString(",");
            }
            var response = await Client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new BusinessException(TwitterAccountDomainErrorCodes.TooManyRequest, "An Error was due to the limit request of Twitter");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new BusinessException(TwitterAccountDomainErrorCodes.Unauthorized, "An Issued with Twitter Access token");
                }

                throw new BusinessException(TwitterAccountDomainErrorCodes.UnexpectedException, "An error occurred while retrieving the information of a user " + username);
            }
            var result = JsonHelper.Parse<TwitterGetUserResponse>(responseContent);
            return result;
        }

        /// <summary>
        /// Lấy thông tin danh sách user theo username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<TwitterGetListUserByUsernameResponse> GetListUserByUsernameAsync(List<string> usernames, params string[] fields)
        {
            if (usernames.IsEmpty())
            {
                throw new ArgumentNullException(nameof(usernames));
            }

            var url = GetListUserByUsernameUrl + usernames.JoinAsString(",");
            if (fields.IsNotEmpty())
            {
                url += $"?user.fields=" + fields.JoinAsString(",");
            }

            var response = await Client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new BusinessException(TwitterAccountDomainErrorCodes.TooManyRequest, "An Error was due to the limit request of Twitter");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new BusinessException(TwitterAccountDomainErrorCodes.Unauthorized, "An Issued with Twitter Access token");
                }

                throw new BusinessException(TwitterAccountDomainErrorCodes.UnexpectedException, "An error occurred while retrieving the information of a users " + usernames);
            }

            var result = JsonHelper.TryParse<TwitterGetListUserByUsernameResponse>(responseContent);
            return result;
        }

        /// <summary>
        /// Lấy thông tin danh sách user theo User id
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<TwitterGetListUserByUsernameResponse> GetListUserByUserIdAsync(List<string> userIds, params string[] fields)
        {
            if (userIds.IsEmpty())
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            var url = GetListUserByUserIdsUrl + userIds.JoinAsString(",");
            if (fields.IsNotEmpty())
            {
                url += $"&user.fields=" + fields.JoinAsString(",");
            }

            var response = await Client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new BusinessException(TwitterAccountDomainErrorCodes.TooManyRequest, "An Error was due to the limit request of Twitter");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new BusinessException(TwitterAccountDomainErrorCodes.Unauthorized, "An Issued with Twitter Access token");
                }

                throw new BusinessException(TwitterAccountDomainErrorCodes.UnexpectedException, "An error occurred while retrieving the information of a users " + userIds);
            }

            var result = JsonHelper.TryParse<TwitterGetListUserByUsernameResponse>(responseContent);
            return result;
        }

    }
}
