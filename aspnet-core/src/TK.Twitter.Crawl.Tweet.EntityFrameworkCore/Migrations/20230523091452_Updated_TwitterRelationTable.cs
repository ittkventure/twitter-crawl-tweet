using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTwitterRelationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "twitter_following_crawl_user");

            migrationBuilder.DropColumn(
                name: "InfoUrl",
                table: "twitter_user");

            migrationBuilder.AddColumn<DateTime>(
                name: "FollowingUserCreatedAt",
                table: "twitter_following_crawl_relation",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowingUserDescription",
                table: "twitter_following_crawl_relation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserFastFollowersCount",
                table: "twitter_following_crawl_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserFavouritesCount",
                table: "twitter_following_crawl_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserFollowersCount",
                table: "twitter_following_crawl_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserFriendsCount",
                table: "twitter_following_crawl_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserListedCount",
                table: "twitter_following_crawl_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FollowingUserName",
                table: "twitter_following_crawl_relation",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserNormalFollowersCount",
                table: "twitter_following_crawl_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FollowingUserProfileImageUrl",
                table: "twitter_following_crawl_relation",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowingUserScreenName",
                table: "twitter_following_crawl_relation",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserStatusesCount",
                table: "twitter_following_crawl_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowingUserCreatedAt",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserDescription",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserFastFollowersCount",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserFavouritesCount",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserFollowersCount",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserFriendsCount",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserListedCount",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserName",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserNormalFollowersCount",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserProfileImageUrl",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserScreenName",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserStatusesCount",
                table: "twitter_following_crawl_relation");

            migrationBuilder.AddColumn<string>(
                name: "InfoUrl",
                table: "twitter_user",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "twitter_following_crawl_user",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    FastFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FavouritesCount = table.Column<int>(type: "integer", nullable: false),
                    FollowersCount = table.Column<int>(type: "integer", nullable: false),
                    FollowingCount = table.Column<int>(type: "integer", nullable: false),
                    FriendsCount = table.Column<int>(type: "integer", nullable: false),
                    InfoUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ListedCount = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalFollowersCount = table.Column<int>(type: "integer", nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ScreenName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TweetCount = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_twitter_following_crawl_user", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_crawl_user_BatchKey_UserId",
                table: "twitter_following_crawl_user",
                columns: new[] { "BatchKey", "UserId" });
        }
    }
}
