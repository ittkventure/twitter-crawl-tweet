using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity.Dapper;
using TK.Twitter.Crawl.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories.Dapper;
using Volo.Abp.EntityFrameworkCore;

namespace TK.Twitter.Crawl.Repository
{
    public class TwitterTweetMentionDapperRepository : DapperRepository<CrawlDbContext>, ITransientDependency, ITwitterTweetMentionDapperRepository
    {
        public TwitterTweetMentionDapperRepository(IDbContextProvider<CrawlDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<PagingResult<TweetMentionDto>> GetMentionListAsync(int pageNumber, int pageSize, string userStatus, string userType, string searchText, string ownerUserScreenName)
        {
            var dbConnection = await GetDbConnectionAsync();
                        
            string sql = @"SELECT 
	mention_main.""UserId"",
	mention_main.""Name"" as ""UserName"",
	mention_main.""ScreenName"" as ""UserScreenName"",
	last_mention.""NumberOfSponsoredTweets"",
	
	lastest_tweet.""TweetId"" as ""LastestTweetId"",		
	lastest_tweet.""LastestSponsoredDate"",	
 	concat('https://twitter.com/_/status/', lastest_tweet.""TweetId"") as ""LastestSponsoredTweetUrl"",	
	lastest_tweet.""TweetDescription"",
	lastest_tweet.""TweetOwnerUserId"",
	lastest_tweet.""UserScreenNameNormalize"" as ""TweetOwnerUserScreenNameNormalize"",
	lastest_tweet.""MentionCount"",
	lastest_tweet.""MediaMentioned"",
	
	user_type.""Type"" as ""UserType"",
	user_status.""Status"" as ""UserStatus""
	
FROM twitter_tweet_user_mention mention_main
-- START JOIN

	JOIN (
				-- Lấy thông tin các mention có max CreationTime và loại bỏ đi các mention có điều kiện không phù hợp
				SELECT
						mention_1.""UserId"",
						""max""(mention_1.""CreationTime"") as ""CreationTime"",
						""count""(mention_1.""UserId"") AS ""NumberOfSponsoredTweets""
				-- 	mention_1.""UserId"", -- for test
				-- 	mention_1.""TweetId"", -- for test
				-- 	tweet_1.""FullText"", -- for test
				-- 	hash_tag.""NormalizeText"" -- for test
				FROM
					twitter_tweet_user_mention mention_1
					JOIN twitter_tweet tweet_1 ON mention_1.""TweetId"" = tweet_1.""TweetId"" 
					JOIN twitter_tweet_hash_tag hash_tag ON tweet_1.""TweetId"" = hash_tag.""TweetId""
				WHERE
					hash_tag.""NormalizeText"" != 'ama'  AND tweet_1.""NormalizeFullText"" not LIKE '%winner%' -- bỏ các tag ama nhưng description lại có chữ winner
					AND mention_1.""UserId"" != '-1' -- bỏ các user đã bị xóa hoặc suspended
					AND mention_1.""UserId"" != tweet_1.""UserId"" -- bỏ các tweet tự mention chính nó
					AND mention_1.""NormalizeScreenName"" NOT IN ('binance', 'coinbase', 'bnbchain', 'epicgames', 'bitfinex', 'bitmartexchange') -- bỏ các tweet mention partner lớn để bú fame
				GROUP BY mention_1.""UserId""
	) as last_mention ON mention_main.""UserId"" = last_mention.""UserId"" AND mention_main.""CreationTime"" = last_mention.""CreationTime""
	
	JOIN (
			-- Lấy thông tin lastest Tweet của mention
			SELECT
				tweet_2.""TweetId"",
				tweet_2.""FullText"" as ""TweetDescription"",
				tweet_2.""NormalizeFullText"",
				tweet_2.""UserScreenName"",
				tweet_2.""UserScreenNameNormalize"",
				tweet_2.""UserId"" as ""TweetOwnerUserId"",
				tweet_2.""CreatedAt"" as ""LastestSponsoredDate"",
				tweet_2.""UserScreenName"" as ""MediaMentioned"",
				mention_2.""MentionCount""
			FROM
				twitter_tweet tweet_2
				JOIN ( SELECT ""TweetId"", ""count"" ( ""UserId"" ) AS ""MentionCount"" FROM twitter_tweet_user_mention GROUP BY ""TweetId"" ) mention_2 ON tweet_2.""TweetId"" = mention_2.""TweetId""		
			WHERE
				tweet_2.""CreatedAt"" is not null -- Thêm điều kiện để sử dụng được index của CreatedAt
			ORDER BY tweet_2.""CreatedAt"" DESC -- Thêm điều kiện để sử dụng được index của CreatedAt
				
	) AS lastest_tweet ON lastest_tweet.""TweetId"" = mention_main.""TweetId""
	
	LEFT JOIN twitter_user_type user_type ON mention_main.""UserId"" = user_type.""UserId""
	LEFT JOIN twitter_user_status user_status ON mention_main.""UserId"" = user_status.""UserId""
-- END JOIN
	
WHERE lastest_tweet.""LastestSponsoredDate"" is not NULL -- Thêm điều kiện để sử dụng được index của CreatedAt
";
            string countSql = @"SELECT 	
  ""count""(mention_main.""UserId"")		
FROM twitter_tweet_user_mention mention_main
	JOIN (
				-- Lấy thông tin các mention có max CreationTime và loại bỏ đi các mention có điều kiện không phù hợp
				SELECT
						mention_1.""UserId"",
						""max""(mention_1.""CreationTime"") as ""CreationTime"",
						""count""(mention_1.""UserId"") AS ""NumberOfSponsoredTweets""
				-- 	mention_1.""UserId"", -- for test
				-- 	mention_1.""TweetId"", -- for test
				-- 	tweet_1.""FullText"", -- for test
				-- 	hash_tag.""NormalizeText"" -- for test
				FROM
					twitter_tweet_user_mention mention_1
					JOIN twitter_tweet tweet_1 ON mention_1.""TweetId"" = tweet_1.""TweetId"" 
					JOIN twitter_tweet_hash_tag hash_tag ON tweet_1.""TweetId"" = hash_tag.""TweetId""
				WHERE
					hash_tag.""NormalizeText"" != 'ama'  AND tweet_1.""NormalizeFullText"" not LIKE '%winner%' -- bỏ các tag ama nhưng description lại có chữ winner
					AND mention_1.""UserId"" != '-1' -- bỏ các user đã bị xóa hoặc suspended
					AND mention_1.""UserId"" != tweet_1.""UserId"" -- bỏ các tweet tự mention chính nó
					AND mention_1.""NormalizeScreenName"" NOT IN ('binance', 'coinbase', 'bnbchain', 'epicgames', 'bitfinex', 'bitmartexchange') -- bỏ các tweet mention partner lớn để bú fame
				GROUP BY mention_1.""UserId""
	) as last_mention ON mention_main.""UserId"" = last_mention.""UserId"" AND mention_main.""CreationTime"" = last_mention.""CreationTime""";

            var param = new
            {
                OFFSET = (pageNumber - 1) * pageSize,
                LIMIT = pageSize,
                USER_STATUS = userStatus,
                USER_TYPE = userType,
                SEARCH_TEXT = searchText,
                OWNER_USER_SCREENNAME = ownerUserScreenName
            };
                       
            
            if (userStatus.IsNotEmpty())
            {
                sql += @" AND user_status.""Status"" = @USER_STATUS";
                countSql += @" AND user_status.""Status"" = @USER_STATUS";
            }

            if (userType.IsNotEmpty())
            {
                sql += @" AND user_status.""Type"" = @USER_TYPE";
                countSql += @" AND user_status.""Type"" = @USER_TYPE";
            }

            if (ownerUserScreenName.IsNotEmpty())
            {
                sql += @" AND lastest_tweet.""UserScreenNameNormalize"" = @OWNER_USER_SCREENNAME";
                countSql += @" AND lastest_tweet.""UserScreenNameNormalize"" = @OWNER_USER_SCREENNAME";
            }

            if (searchText.IsNotEmpty())
            {
                sql += @" AND (lastest_tweet.""NormalizeFullText"" like '%@SEARCH_TEXT' || mention_main.""NormalizeUserScreenName"" like '%@SEARCH_TEXT')";
                countSql += @" AND (lastest_tweet.""NormalizeFullText"" like '%@SEARCH_TEXT' || mention_main.""NormalizeUserScreenName"" like '%@SEARCH_TEXT')";
            }

            sql += @"ORDER BY lastest_tweet.""LastestSponsoredDate"" DESC -- Thêm điều kiện để sử dụng được index của CreatedAt";
            sql += @"	LIMIT @LIMIT OFFSET @OFFSET";
            var pagingResult = new PagingResult<TweetMentionDto>();            
            pagingResult.TotalCount = await dbConnection.QueryFirstAsync<int>(countSql, param, transaction: await GetDbTransactionAsync());
            if (pagingResult.TotalCount == 0)
            {
                return pagingResult;
            }

            var temp = await dbConnection.QueryAsync<TweetMentionDto>(sql, param, transaction: await GetDbTransactionAsync());
            pagingResult.Items = temp?.ToList();

            return pagingResult;
        }
    }
}
