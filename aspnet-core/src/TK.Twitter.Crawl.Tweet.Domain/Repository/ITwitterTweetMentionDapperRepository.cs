using System.Collections.Generic;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity.Dapper;

namespace TK.Twitter.Crawl.Repository
{
    public interface ITwitterTweetMentionDapperRepository
    {
        Task<PagingResult<TweetMentionDto>> GetMentionListAsync(int pageNumber, int pageSize, string userStatus, string userType, string searchText, string ownerUserScreenName);
    }
}