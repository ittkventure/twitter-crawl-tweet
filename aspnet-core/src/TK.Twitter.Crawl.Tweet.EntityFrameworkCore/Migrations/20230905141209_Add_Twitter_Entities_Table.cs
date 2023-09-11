using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class AddTwitterEntitiesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_twitter_crawl_tweet",
                table: "twitter_crawl_tweet");

            migrationBuilder.DropColumn(
                name: "EntitiesAsJson",
                table: "twitter_crawl_tweet");

            migrationBuilder.DropColumn(
                name: "ExtendedEntitiesAsJson",
                table: "twitter_crawl_tweet");

            migrationBuilder.DropColumn(
                name: "QuoteStatusResultAsJson",
                table: "twitter_crawl_tweet");

            migrationBuilder.DropColumn(
                name: "UserResultAsJson",
                table: "twitter_crawl_tweet");

            migrationBuilder.RenameTable(
                name: "twitter_crawl_tweet",
                newName: "twitter_tweet");

            migrationBuilder.RenameIndex(
                name: "IX_twitter_crawl_tweet_UserId",
                table: "twitter_tweet",
                newName: "IX_twitter_tweet_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_twitter_crawl_tweet_TweetId",
                table: "twitter_tweet",
                newName: "IX_twitter_tweet_TweetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_twitter_tweet",
                table: "twitter_tweet",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "twitter_tweet_crawl_raw",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TweetId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    JsonContent = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_twitter_tweet_crawl_raw", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_tweet_hash_tag",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TweetId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Text = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_twitter_tweet_hash_tag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_tweet_media",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TweetId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DisplayUrl = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExpandedUrl = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MediaId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    MediaUrlHttps = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
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
                    table.PrimaryKey("PK_twitter_tweet_media", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_tweet_symbol",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TweetId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SymbolText = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_twitter_tweet_symbol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_tweet_url",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TweetId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DisplayUrl = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExpandedUrl = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_twitter_tweet_url", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_tweet_user_mention",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TweetId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ScreenName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_twitter_tweet_user_mention", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_crawl_raw_TweetId",
                table: "twitter_tweet_crawl_raw",
                column: "TweetId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_hash_tag_TweetId",
                table: "twitter_tweet_hash_tag",
                column: "TweetId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_media_TweetId",
                table: "twitter_tweet_media",
                column: "TweetId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_symbol_TweetId",
                table: "twitter_tweet_symbol",
                column: "TweetId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_url_TweetId",
                table: "twitter_tweet_url",
                column: "TweetId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_user_mention_TweetId",
                table: "twitter_tweet_user_mention",
                column: "TweetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "twitter_tweet_crawl_raw");

            migrationBuilder.DropTable(
                name: "twitter_tweet_hash_tag");

            migrationBuilder.DropTable(
                name: "twitter_tweet_media");

            migrationBuilder.DropTable(
                name: "twitter_tweet_symbol");

            migrationBuilder.DropTable(
                name: "twitter_tweet_url");

            migrationBuilder.DropTable(
                name: "twitter_tweet_user_mention");

            migrationBuilder.DropPrimaryKey(
                name: "PK_twitter_tweet",
                table: "twitter_tweet");

            migrationBuilder.RenameTable(
                name: "twitter_tweet",
                newName: "twitter_crawl_tweet");

            migrationBuilder.RenameIndex(
                name: "IX_twitter_tweet_UserId",
                table: "twitter_crawl_tweet",
                newName: "IX_twitter_crawl_tweet_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_twitter_tweet_TweetId",
                table: "twitter_crawl_tweet",
                newName: "IX_twitter_crawl_tweet_TweetId");

            migrationBuilder.AddColumn<string>(
                name: "EntitiesAsJson",
                table: "twitter_crawl_tweet",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtendedEntitiesAsJson",
                table: "twitter_crawl_tweet",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuoteStatusResultAsJson",
                table: "twitter_crawl_tweet",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserResultAsJson",
                table: "twitter_crawl_tweet",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_twitter_crawl_tweet",
                table: "twitter_crawl_tweet",
                column: "Id");
        }
    }
}
