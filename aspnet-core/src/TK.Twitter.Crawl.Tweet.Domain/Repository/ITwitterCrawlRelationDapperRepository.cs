using System.Collections.Generic;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity.Dapper;

namespace TK.Twitter.Crawl.Repository
{
    public interface ITwitterCrawlRelationDapperRepository
    {
        Task CreateTable(string batchKey);
        Task DropTable(string batchKey);
        Task<List<TwitterTweetCrawlTweetDapperEntity>> GetByFollowingUserId(string batchKey, string userId);
        Task<List<TwitterTweetCrawlTweetDapperEntity>> GetByFollowingUserIds(string batchKey, List<string> userIds);
        Task<List<TwitterTweetCrawlTweetDapperEntity>> GetByUserId(string batchKey, string userId);
        Task<List<TwitterTweetCrawlTweetDapperEntity>> GetByUserIds(string batchKey, List<string> userIds);
        Task<int> CountUnfollowedUser(string batchKey);
        Task<List<string>> GetUnfollowedUsers(string batchKey, int pageIndex, int pageSize);
        Task InsertAsync(string batchKey, TwitterTweetCrawlTweetDapperEntity entity);
    }
}