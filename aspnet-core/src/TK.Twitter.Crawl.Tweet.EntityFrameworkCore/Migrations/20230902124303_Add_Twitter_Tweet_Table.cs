using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class AddTwitterTweetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "twitter_following_check_relation_queue");

            migrationBuilder.DropTable(
                name: "twitter_following_crawl_distributed_lock");

            migrationBuilder.DropTable(
                name: "twitter_following_crawl_queue");

            migrationBuilder.DropTable(
                name: "twitter_following_crawl_relation");

            migrationBuilder.DropTable(
                name: "twitter_user");

            migrationBuilder.DropTable(
                name: "twitter_user_change_log");

            migrationBuilder.DropTable(
                name: "twitter_user_crawl_category");

            migrationBuilder.DropTable(
                name: "twitter_user_data_change");

            migrationBuilder.DropTable(
                name: "twitter_user_event");

            migrationBuilder.DropTable(
                name: "twitter_user_metric");

            migrationBuilder.DropTable(
                name: "twitter_user_metric_current");

            migrationBuilder.DropTable(
                name: "twitter_user_relation");

            migrationBuilder.DropTable(
                name: "twitter_user_unavailable");

            migrationBuilder.DropTable(
                name: "twitter_user_url");

            migrationBuilder.DropTable(
                name: "twitter_following_crawl_batch");

            migrationBuilder.CreateTable(
                name: "twitter_crawl_tweet",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TweetId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ViewsCount = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    BookmarkCount = table.Column<int>(type: "integer", nullable: false),
                    FavoriteCount = table.Column<int>(type: "integer", nullable: false),
                    QuoteCount = table.Column<int>(type: "integer", nullable: false),
                    ReplyCount = table.Column<int>(type: "integer", nullable: false),
                    RetweetCount = table.Column<int>(type: "integer", nullable: false),
                    UserResultAsJson = table.Column<string>(type: "text", nullable: true),
                    EntitiesAsJson = table.Column<string>(type: "text", nullable: true),
                    ExtendedEntitiesAsJson = table.Column<string>(type: "text", nullable: true),
                    QuoteStatusResultAsJson = table.Column<string>(type: "text", nullable: true),
                    FullText = table.Column<string>(type: "text", nullable: true),
                    IsQuoteStatus = table.Column<bool>(type: "boolean", nullable: false),
                    Lang = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    InReplyToScreenName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InReplyToStatusId = table.Column<string>(type: "character varying(56)", maxLength: 56, nullable: true),
                    InReplyToUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ConversationId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_crawl_tweet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_tweet_crawl_batch",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CrawlTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Key = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_tweet_crawl_batch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_tweet_crawl_queue",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TwitterAccountId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    BatchKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Ended = table.Column<bool>(type: "boolean", nullable: false),
                    Successed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessedAttempt = table.Column<int>(type: "integer", nullable: false),
                    ErrorProcessedAttempt = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CurrentCursor = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TwitterTweetCrawlBatchEntityId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_tweet_crawl_queue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_twitter_tweet_crawl_queue_twitter_tweet_crawl_batch_Twitter~",
                        column: x => x.TwitterTweetCrawlBatchEntityId,
                        principalTable: "twitter_tweet_crawl_batch",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_crawl_tweet_TweetId",
                table: "twitter_crawl_tweet",
                column: "TweetId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_crawl_tweet_UserId",
                table: "twitter_crawl_tweet",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_crawl_batch_CrawlTime",
                table: "twitter_tweet_crawl_batch",
                column: "CrawlTime");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_crawl_batch_Key",
                table: "twitter_tweet_crawl_batch",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_crawl_queue_BatchKey_UserId_Ended_TwitterAcco~",
                table: "twitter_tweet_crawl_queue",
                columns: new[] { "BatchKey", "UserId", "Ended", "TwitterAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_crawl_queue_TwitterTweetCrawlBatchEntityId",
                table: "twitter_tweet_crawl_queue",
                column: "TwitterTweetCrawlBatchEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "twitter_crawl_tweet");

            migrationBuilder.DropTable(
                name: "twitter_tweet_crawl_queue");

            migrationBuilder.DropTable(
                name: "twitter_tweet_crawl_batch");

            migrationBuilder.CreateTable(
                name: "twitter_following_check_relation_queue",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Ended = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ProcessedAttempt = table.Column<int>(type: "integer", nullable: false),
                    Successed = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_following_check_relation_queue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_following_crawl_batch",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CrawlTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Key = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_following_crawl_batch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_following_crawl_distributed_lock",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    JobKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Locked = table.Column<bool>(type: "boolean", nullable: false),
                    LockedByBatchKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    NextExecutionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TwitterAccountId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_following_crawl_distributed_lock", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_following_crawl_relation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DiscoveredTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EntitiesAsJson = table.Column<string>(type: "text", nullable: true),
                    FollowingUserCreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FollowingUserDescription = table.Column<string>(type: "text", nullable: true),
                    FollowingUserFastFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserFavouritesCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserFriendsCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FollowingUserListedCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FollowingUserNormalFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserProfileImageUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    FollowingUserScreenName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FollowingUserStatusesCount = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProfessionalAsJson = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_following_crawl_relation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DiscoveredTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ScreenName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TwitterUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_change_log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    DataTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_user_change_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_crawl_category",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IconName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TwitterId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_user_crawl_category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_data_change",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FieldName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FromTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    OriginalData = table.Column<string>(type: "text", nullable: true),
                    ScanTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ToTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_user_data_change", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EventTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_user_event", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_metric",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FastFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FastFollowersCountChange = table.Column<int>(type: "integer", nullable: false),
                    FavouritesCount = table.Column<int>(type: "integer", nullable: false),
                    FavouritesCountChange = table.Column<int>(type: "integer", nullable: false),
                    FollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowersCountChange = table.Column<int>(type: "integer", nullable: false),
                    FollowingCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingCountChange = table.Column<int>(type: "integer", nullable: false),
                    FriendsCount = table.Column<int>(type: "integer", nullable: false),
                    FriendsCountChange = table.Column<int>(type: "integer", nullable: false),
                    FromTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ListedCount = table.Column<int>(type: "integer", nullable: false),
                    ListedCountChange = table.Column<int>(type: "integer", nullable: false),
                    NormalFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    NormalFollowersCountChange = table.Column<int>(type: "integer", nullable: false),
                    ScanTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ToTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TweetCount = table.Column<int>(type: "integer", nullable: false),
                    TweetCountChange = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_user_metric", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_metric_current",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FastFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FastFollowersCountChange = table.Column<int>(type: "integer", nullable: false),
                    FavouritesCount = table.Column<int>(type: "integer", nullable: false),
                    FavouritesCountChange = table.Column<int>(type: "integer", nullable: false),
                    FollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowersCountChange = table.Column<int>(type: "integer", nullable: false),
                    FollowingCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingCountChange = table.Column<int>(type: "integer", nullable: false),
                    FriendsCount = table.Column<int>(type: "integer", nullable: false),
                    FriendsCountChange = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ListedCount = table.Column<int>(type: "integer", nullable: false),
                    ListedCountChange = table.Column<int>(type: "integer", nullable: false),
                    NormalFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    NormalFollowersCountChange = table.Column<int>(type: "integer", nullable: false),
                    ScanTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TweetCount = table.Column<int>(type: "integer", nullable: false),
                    TweetCountChange = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_user_metric_current", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_relation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FollowingTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FollowingUserCreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FollowingUserDescription = table.Column<string>(type: "text", nullable: true),
                    FollowingUserFastFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserFavouritesCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserFriendsCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FollowingUserListedCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FollowingUserNormalFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingUserProfileImageUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    FollowingUserScreenName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FollowingUserStatusesCount = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_user_relation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_unavailable",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_user_unavailable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_url",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DisplayUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ExpandedUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_user_url", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_following_crawl_queue",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentCursor = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Ended = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorProcessedAttempt = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ProcessedAttempt = table.Column<int>(type: "integer", nullable: false),
                    Successed = table.Column<bool>(type: "boolean", nullable: false),
                    TwitterAccountId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TwitterFollowingCrawlBatchEntityId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_following_crawl_queue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_twitter_following_crawl_queue_twitter_following_crawl_batch~",
                        column: x => x.TwitterFollowingCrawlBatchEntityId,
                        principalTable: "twitter_following_crawl_batch",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_check_relation_queue_BatchKey_UserId_Ended",
                table: "twitter_following_check_relation_queue",
                columns: new[] { "BatchKey", "UserId", "Ended" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_crawl_batch_CrawlTime",
                table: "twitter_following_crawl_batch",
                column: "CrawlTime");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_crawl_batch_Key",
                table: "twitter_following_crawl_batch",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_crawl_queue_BatchKey_UserId_Ended_Twitter~",
                table: "twitter_following_crawl_queue",
                columns: new[] { "BatchKey", "UserId", "Ended", "TwitterAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_crawl_queue_TwitterFollowingCrawlBatchEnt~",
                table: "twitter_following_crawl_queue",
                column: "TwitterFollowingCrawlBatchEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_crawl_relation_BatchKey_FollowingUserId",
                table: "twitter_following_crawl_relation",
                columns: new[] { "BatchKey", "FollowingUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_crawl_relation_BatchKey_UserId",
                table: "twitter_following_crawl_relation",
                columns: new[] { "BatchKey", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_CreatedAt",
                table: "twitter_user",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_DiscoveredTime",
                table: "twitter_user",
                column: "DiscoveredTime");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_UserId",
                table: "twitter_user",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_change_log_UserId",
                table: "twitter_user_change_log",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_change_log_UserId_DataType",
                table: "twitter_user_change_log",
                columns: new[] { "UserId", "DataType" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_crawl_category_UserId",
                table: "twitter_user_crawl_category",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_data_change_UserId",
                table: "twitter_user_data_change",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_data_change_UserId_FieldName_IsCurrent",
                table: "twitter_user_data_change",
                columns: new[] { "UserId", "FieldName", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_event_TargetUserId",
                table: "twitter_user_event",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_event_UserId",
                table: "twitter_user_event",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_metric_UserId_IsCurrent",
                table: "twitter_user_metric",
                columns: new[] { "UserId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_metric_current_UserId",
                table: "twitter_user_metric_current",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_relation_UserId",
                table: "twitter_user_relation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_unavailable_UserId",
                table: "twitter_user_unavailable",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_url_UserId",
                table: "twitter_user_url",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_url_UserId_Type",
                table: "twitter_user_url",
                columns: new[] { "UserId", "Type" });
        }
    }
}
