using Dapper;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity.Dapper;
using TK.Twitter.Crawl.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories.Dapper;
using Volo.Abp.EntityFrameworkCore;

namespace TK.Twitter.Crawl.Repository
{
    public class TwitterCrawlRelationDapperRepository : DapperRepository<CrawlDbContext>, ITransientDependency, ITwitterCrawlRelationDapperRepository
    {
        private class Column
        {
            public string Name { get; set; }

            public NpgsqlDbType NpgDbType { get; set; }

            public bool IsNullable { get; set; }
        }

        private static readonly List<Column> Columns = new()
        {
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.UserId), NpgDbType = NpgsqlDbType.Text, IsNullable = true },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserId), NpgDbType = NpgsqlDbType.Text, IsNullable = true },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserName), NpgDbType = NpgsqlDbType.Text, IsNullable = true },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserScreenName), NpgDbType = NpgsqlDbType.Text, IsNullable = true },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserCreatedAt), NpgDbType = NpgsqlDbType.Timestamp, IsNullable = true },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserDescription), NpgDbType = NpgsqlDbType.Text, IsNullable = true },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserProfileImageUrl), NpgDbType = NpgsqlDbType.Text, IsNullable = true },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserFavouritesCount), NpgDbType = NpgsqlDbType.Integer},
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserFastFollowersCount), NpgDbType = NpgsqlDbType.Integer},
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserFollowersCount), NpgDbType = NpgsqlDbType.Integer },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserFriendsCount), NpgDbType = NpgsqlDbType.Integer },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserListedCount), NpgDbType = NpgsqlDbType.Integer },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserNormalFollowersCount), NpgDbType = NpgsqlDbType.Integer },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.FollowingUserStatusesCount), NpgDbType = NpgsqlDbType.Integer },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.DiscoveredTime), NpgDbType = NpgsqlDbType.Timestamp },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.EntitiesAsJson), NpgDbType = NpgsqlDbType.Text, IsNullable = true },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.ProfessionalAsJson), NpgDbType = NpgsqlDbType.Text, IsNullable = true },
            new() { Name = nameof(TwitterTweetCrawlTweetDapperEntity.CreationTime), NpgDbType = NpgsqlDbType.Timestamp},
        };

        public TwitterCrawlRelationDapperRepository(IDbContextProvider<CrawlDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        private static string GetTableName(string batchKey)
        {
            return $"twitter_following_crawl_relation_{batchKey}";
        }

        public async Task CreateTable(string batchKey)
        {
            string table = GetTableName(batchKey);
            var dbConnection = await GetDbConnectionAsync();
            string sql = @$"
DROP TABLE IF EXISTS ""public"".""{table}"";
DROP INDEX IF EXISTS  ""IX_{table}_FollowingUserId"";
DROP INDEX IF EXISTS  ""IX_{table}_UserId"";

CREATE TABLE ""public"".""{table}"" 
 (""Id"" ""bigserial"" NOT NULL PRIMARY KEY,
  ""UserId"" ""pg_catalog"".""varchar"" COLLATE ""pg_catalog"".""default"",
  ""FollowingUserId"" ""pg_catalog"".""varchar"" COLLATE ""pg_catalog"".""default"",
  ""CreationTime"" ""pg_catalog"".""timestamp"" NOT NULL, 
  ""FollowingUserCreatedAt"" ""pg_catalog"".""timestamp"",
  ""FollowingUserDescription"" ""pg_catalog"".""text"" COLLATE ""pg_catalog"".""default"",
  ""FollowingUserFastFollowersCount"" ""pg_catalog"".""int4"" NOT NULL DEFAULT 0,
  ""FollowingUserFavouritesCount"" ""pg_catalog"".""int4"" NOT NULL DEFAULT 0,
  ""FollowingUserFollowersCount"" ""pg_catalog"".""int4"" NOT NULL DEFAULT 0,
  ""FollowingUserFriendsCount"" ""pg_catalog"".""int4"" NOT NULL DEFAULT 0,
  ""FollowingUserListedCount"" ""pg_catalog"".""int4"" NOT NULL DEFAULT 0,
  ""FollowingUserName"" ""pg_catalog"".""varchar"" COLLATE ""pg_catalog"".""default"",
  ""FollowingUserNormalFollowersCount"" ""pg_catalog"".""int4"" NOT NULL DEFAULT 0,
  ""FollowingUserProfileImageUrl"" ""pg_catalog"".""varchar"" COLLATE ""pg_catalog"".""default"",
  ""FollowingUserScreenName"" ""pg_catalog"".""varchar"" COLLATE ""pg_catalog"".""default"",
  ""FollowingUserStatusesCount"" ""pg_catalog"".""int4"" NOT NULL DEFAULT 0,
  ""DiscoveredTime"" ""pg_catalog"".""timestamp"" NOT NULL DEFAULT '-infinity'::timestamp without time zone,
  ""EntitiesAsJson"" ""pg_catalog"".""text"" COLLATE ""pg_catalog"".""default"",
  ""ProfessionalAsJson"" ""pg_catalog"".""text"" COLLATE ""pg_catalog"".""default""
)
;

CREATE INDEX ""IX_{table}_FollowingUserId"" ON ""public"".""{table}"" USING btree (
  ""FollowingUserId"" COLLATE ""pg_catalog"".""default"" ""pg_catalog"".""text_ops"" ASC NULLS LAST
);

CREATE INDEX ""IX_{table}_UserId"" ON ""public"".""{table}"" USING btree (
  ""UserId"" COLLATE ""pg_catalog"".""default"" ""pg_catalog"".""text_ops"" ASC NULLS LAST
);
";
            await dbConnection.ExecuteAsync(sql, transaction: await GetDbTransactionAsync());
        }

        public async Task DropTable(string batchKey)
        {
            string table = GetTableName(batchKey);
            var dbConnection = await GetDbConnectionAsync();
            string sql = @$"
DROP TABLE IF EXISTS ""public"".""{table}"";
DROP INDEX IF EXISTS  ""IX_{table}_FollowingUserId"";
DROP INDEX IF EXISTS  ""IX_{table}_UserId"";";
            await dbConnection.ExecuteAsync(sql, transaction: await GetDbTransactionAsync());
        }

        public async Task InsertAsync(string batchKey, TwitterTweetCrawlTweetDapperEntity entity)
        {
            string table = GetTableName(batchKey);
            var dbConnection = await GetDbConnectionAsync();

            StringBuilder sb = new($"INSERT INTO \"public\".\"{table}\" (");
            for (int i = 0; i < Columns.Count; i++)
            {
                var col = Columns[i];
                sb.Append($"\"{col.Name}\"");
                if (i != Columns.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append(") VALUES ( ");
            for (int i = 0; i < Columns.Count; i++)
            {
                var col = Columns[i];
                sb.Append($"@{col.Name}");
                if (i != Columns.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append(")");

            string sql = sb.ToString();
            await dbConnection.ExecuteAsync(sql, param: entity, transaction: await GetDbTransactionAsync());
        }

        public async Task<List<TwitterTweetCrawlTweetDapperEntity>> GetByUserId(string batchKey, string userId)
        {
            string table = GetTableName(batchKey);
            var dbConnection = await GetDbConnectionAsync();
            string sql = $"SELECT * FROM \"{table}\" WHERE \"UserId\" = @UserId";
            var temp = await dbConnection.QueryAsync<TwitterTweetCrawlTweetDapperEntity>(sql, new { UserId = userId }, transaction: await GetDbTransactionAsync());
            return temp?.ToList();
        }

        public async Task<List<TwitterTweetCrawlTweetDapperEntity>> GetByUserIds(string batchKey, List<string> userIds)
        {
            string table = GetTableName(batchKey);
            var dbConnection = await GetDbConnectionAsync();
            string sql = $"SELECT * FROM \"{table}\" WHERE \"UserId\" IN ('{userIds.JoinAsString("','")}')";
            var temp = await dbConnection.QueryAsync<TwitterTweetCrawlTweetDapperEntity>(sql, transaction: await GetDbTransactionAsync());
            return temp?.ToList();
        }

        public async Task<List<TwitterTweetCrawlTweetDapperEntity>> GetByFollowingUserId(string batchKey, string userId)
        {
            string table = GetTableName(batchKey);
            var dbConnection = await GetDbConnectionAsync();
            string sql = $"SELECT * FROM \"{table}\" WHERE \"FollowingUserId\" = @FollowingUserId";
            var temp = await dbConnection.QueryAsync<TwitterTweetCrawlTweetDapperEntity>(sql, new { FollowingUserId = userId }, transaction: await GetDbTransactionAsync());
            return temp?.ToList();
        }

        public async Task<List<TwitterTweetCrawlTweetDapperEntity>> GetByFollowingUserIds(string batchKey, List<string> userIds)
        {
            string table = GetTableName(batchKey);
            var dbConnection = await GetDbConnectionAsync();
            string sql = $"SELECT * FROM \"{table}\" WHERE \"FollowingUserId\" IN ('{userIds.JoinAsString("','")}')";
            var temp = await dbConnection.QueryAsync<TwitterTweetCrawlTweetDapperEntity>(sql, transaction: await GetDbTransactionAsync());
            return temp?.ToList();
        }

        public async Task<int> CountUnfollowedUser(string batchKey)
        {
            string table = GetTableName(batchKey);
            var dbConnection = await GetDbConnectionAsync();
            string sql = @$"SELECT COUNT(u.""UserId"") FROM twitter_user u
WHERE NOT EXISTS (SELECT 1 FROM {table} crawl WHERE u.""UserId"" = crawl.""FollowingUserId"")
-- AND NOT EXISTS (SELECT 1 FROM twitter_user_unavailable uu WHERE u.""UserId"" = uu.""UserId"");";
            var count = await dbConnection.ExecuteScalarAsync<int>(sql, transaction: await GetDbTransactionAsync());
            return count;
        }

        public async Task<List<string>> GetUnfollowedUsers(string batchKey, int skip, int take)
        {
            string table = GetTableName(batchKey);
            var dbConnection = await GetDbConnectionAsync();
            string sql = @$"SELECT u.""UserId"" FROM twitter_user u
WHERE NOT EXISTS (SELECT 1 FROM {table} crawl WHERE u.""UserId"" = crawl.""FollowingUserId"")
-- AND NOT EXISTS (SELECT 1 FROM twitter_user_unavailable uu WHERE u.""UserId"" = uu.""UserId"") 
LIMIT {take} OFFSET {skip};";
            var temp = await dbConnection.QueryAsync<string>(sql, transaction: await GetDbTransactionAsync());
            return temp?.ToList();
        }
    }
}
