using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class AddedTwitterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "twitter_account",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    APIKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    APIKeySecret = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_twitter_account", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_account_api",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    MaxRequestsPerWindowTime = table.Column<int>(type: "integer", nullable: false),
                    WindowTimeAsMinute = table.Column<int>(type: "integer", nullable: false),
                    HasReachedLimit = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_twitter_account_api", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_crawl_account",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CookieCtZeroValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    GuestToken = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Cookie = table.Column<string>(type: "character varying(2650)", maxLength: 2650, nullable: true),
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
                    table.PrimaryKey("PK_twitter_crawl_account", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_following_check_relation_queue",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Ended = table.Column<bool>(type: "boolean", nullable: false),
                    Successed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessedAttempt = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_twitter_following_check_relation_queue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_following_crawl_batch",
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
                    table.PrimaryKey("PK_twitter_following_crawl_batch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_following_crawl_distributed_lock",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Locked = table.Column<bool>(type: "boolean", nullable: false),
                    JobKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TwitterAccountId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NextExecutionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LockedByBatchKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
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
                    table.PrimaryKey("PK_twitter_following_crawl_distributed_lock", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    InfoUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TwitterUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DiscoveredTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                    table.PrimaryKey("PK_twitter_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_event",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TargetUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EventTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                    table.PrimaryKey("PK_twitter_user_event", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_metric",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ScanTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FromTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ToTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    FollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowersCountChange = table.Column<int>(type: "integer", nullable: false),
                    FollowingCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingCountChange = table.Column<int>(type: "integer", nullable: false),
                    TweetCount = table.Column<int>(type: "integer", nullable: false),
                    TweetCountChange = table.Column<int>(type: "integer", nullable: false),
                    ListedCount = table.Column<int>(type: "integer", nullable: false),
                    ListedCountChange = table.Column<int>(type: "integer", nullable: false),
                    FastFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FastFollowersCountChange = table.Column<int>(type: "integer", nullable: false),
                    FavouritesCount = table.Column<int>(type: "integer", nullable: false),
                    FavouritesCountChange = table.Column<int>(type: "integer", nullable: false),
                    FriendsCount = table.Column<int>(type: "integer", nullable: false),
                    FriendsCountChange = table.Column<int>(type: "integer", nullable: false),
                    NormalFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    NormalFollowersCountChange = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_twitter_user_metric", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_metric_current",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ScanTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowersCountChange = table.Column<int>(type: "integer", nullable: false),
                    FollowingCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingCountChange = table.Column<int>(type: "integer", nullable: false),
                    TweetCount = table.Column<int>(type: "integer", nullable: false),
                    TweetCountChange = table.Column<int>(type: "integer", nullable: false),
                    ListedCount = table.Column<int>(type: "integer", nullable: false),
                    ListedCountChange = table.Column<int>(type: "integer", nullable: false),
                    FastFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FastFollowersCountChange = table.Column<int>(type: "integer", nullable: false),
                    FavouritesCount = table.Column<int>(type: "integer", nullable: false),
                    FavouritesCountChange = table.Column<int>(type: "integer", nullable: false),
                    FriendsCount = table.Column<int>(type: "integer", nullable: false),
                    FriendsCountChange = table.Column<int>(type: "integer", nullable: false),
                    NormalFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    NormalFollowersCountChange = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_twitter_user_metric_current", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_relation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FollowingUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FollowingTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                    table.PrimaryKey("PK_twitter_user_relation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_following_crawl_queue",
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
                    Note = table.Column<string>(type: "text", nullable: true),
                    TwitterFollowingCrawlBatchEntityId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_twitter_following_crawl_queue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_twitter_following_crawl_queue_twitter_following_crawl_batch~",
                        column: x => x.TwitterFollowingCrawlBatchEntityId,
                        principalTable: "twitter_following_crawl_batch",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "twitter_following_crawl_relation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FollowingUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TwitterFollowingCrawlBatchEntityId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_twitter_following_crawl_relation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_twitter_following_crawl_relation_twitter_following_crawl_ba~",
                        column: x => x.TwitterFollowingCrawlBatchEntityId,
                        principalTable: "twitter_following_crawl_batch",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "twitter_following_crawl_user",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InfoUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingCount = table.Column<int>(type: "integer", nullable: false),
                    TweetCount = table.Column<int>(type: "integer", nullable: false),
                    ListedCount = table.Column<int>(type: "integer", nullable: false),
                    FastFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FavouritesCount = table.Column<int>(type: "integer", nullable: false),
                    FriendsCount = table.Column<int>(type: "integer", nullable: false),
                    NormalFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    TwitterFollowingCrawlBatchEntityId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_twitter_following_crawl_user", x => x.Id);
                    table.ForeignKey(
                        name: "FK_twitter_following_crawl_user_twitter_following_crawl_batch_~",
                        column: x => x.TwitterFollowingCrawlBatchEntityId,
                        principalTable: "twitter_following_crawl_batch",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_account_AccountId",
                table: "twitter_account",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_account_api_Key_AccountId",
                table: "twitter_account_api",
                columns: new[] { "Key", "AccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_crawl_account_AccountId",
                table: "twitter_crawl_account",
                column: "AccountId");

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
                name: "IX_twitter_following_crawl_relation_TwitterFollowingCrawlBatch~",
                table: "twitter_following_crawl_relation",
                column: "TwitterFollowingCrawlBatchEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_crawl_user_BatchKey_UserId",
                table: "twitter_following_crawl_user",
                columns: new[] { "BatchKey", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_crawl_user_TwitterFollowingCrawlBatchEnti~",
                table: "twitter_following_crawl_user",
                column: "TwitterFollowingCrawlBatchEntityId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "twitter_account");

            migrationBuilder.DropTable(
                name: "twitter_account_api");

            migrationBuilder.DropTable(
                name: "twitter_crawl_account");

            migrationBuilder.DropTable(
                name: "twitter_following_check_relation_queue");

            migrationBuilder.DropTable(
                name: "twitter_following_crawl_distributed_lock");

            migrationBuilder.DropTable(
                name: "twitter_following_crawl_queue");

            migrationBuilder.DropTable(
                name: "twitter_following_crawl_relation");

            migrationBuilder.DropTable(
                name: "twitter_following_crawl_user");

            migrationBuilder.DropTable(
                name: "twitter_user");

            migrationBuilder.DropTable(
                name: "twitter_user_event");

            migrationBuilder.DropTable(
                name: "twitter_user_metric");

            migrationBuilder.DropTable(
                name: "twitter_user_metric_current");

            migrationBuilder.DropTable(
                name: "twitter_user_relation");

            migrationBuilder.DropTable(
                name: "twitter_following_crawl_batch");
        }
    }
}
