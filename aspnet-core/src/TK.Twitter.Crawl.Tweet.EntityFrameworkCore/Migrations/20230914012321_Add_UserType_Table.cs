using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizeName",
                table: "twitter_tweet_user_mention",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizeScreenName",
                table: "twitter_tweet_user_mention",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizeText",
                table: "twitter_tweet_hash_tag",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizeFullText",
                table: "twitter_tweet",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "twitter_user_status",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Status = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_twitter_user_status", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_type",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_twitter_user_type", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_user_mention_TweetId_CreationTime",
                table: "twitter_tweet_user_mention",
                columns: new[] { "TweetId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_user_mention_TweetId_NormalizeScreenName",
                table: "twitter_tweet_user_mention",
                columns: new[] { "TweetId", "NormalizeScreenName" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_user_mention_TweetId_ScreenName",
                table: "twitter_tweet_user_mention",
                columns: new[] { "TweetId", "ScreenName" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_hash_tag_TweetId_NormalizeText",
                table: "twitter_tweet_hash_tag",
                columns: new[] { "TweetId", "NormalizeText" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_status_UserId",
                table: "twitter_user_status",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_type_UserId",
                table: "twitter_user_type",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "twitter_user_status");

            migrationBuilder.DropTable(
                name: "twitter_user_type");

            migrationBuilder.DropIndex(
                name: "IX_twitter_tweet_user_mention_TweetId_CreationTime",
                table: "twitter_tweet_user_mention");

            migrationBuilder.DropIndex(
                name: "IX_twitter_tweet_user_mention_TweetId_NormalizeScreenName",
                table: "twitter_tweet_user_mention");

            migrationBuilder.DropIndex(
                name: "IX_twitter_tweet_user_mention_TweetId_ScreenName",
                table: "twitter_tweet_user_mention");

            migrationBuilder.DropIndex(
                name: "IX_twitter_tweet_hash_tag_TweetId_NormalizeText",
                table: "twitter_tweet_hash_tag");

            migrationBuilder.DropColumn(
                name: "NormalizeName",
                table: "twitter_tweet_user_mention");

            migrationBuilder.DropColumn(
                name: "NormalizeScreenName",
                table: "twitter_tweet_user_mention");

            migrationBuilder.DropColumn(
                name: "NormalizeText",
                table: "twitter_tweet_hash_tag");

            migrationBuilder.DropColumn(
                name: "NormalizeFullText",
                table: "twitter_tweet");
        }
    }
}
