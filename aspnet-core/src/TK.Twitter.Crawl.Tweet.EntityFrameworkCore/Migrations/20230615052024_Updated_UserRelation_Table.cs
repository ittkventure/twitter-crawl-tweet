using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedUserRelationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FollowingUserCreatedAt",
                table: "twitter_user_relation",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowingUserDescription",
                table: "twitter_user_relation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserFastFollowersCount",
                table: "twitter_user_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserFavouritesCount",
                table: "twitter_user_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserFollowersCount",
                table: "twitter_user_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserFriendsCount",
                table: "twitter_user_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserListedCount",
                table: "twitter_user_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FollowingUserName",
                table: "twitter_user_relation",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserNormalFollowersCount",
                table: "twitter_user_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FollowingUserProfileImageUrl",
                table: "twitter_user_relation",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowingUserScreenName",
                table: "twitter_user_relation",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FollowingUserStatusesCount",
                table: "twitter_user_relation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowingUserCreatedAt",
                table: "twitter_user_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserDescription",
                table: "twitter_user_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserFastFollowersCount",
                table: "twitter_user_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserFavouritesCount",
                table: "twitter_user_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserFollowersCount",
                table: "twitter_user_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserFriendsCount",
                table: "twitter_user_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserListedCount",
                table: "twitter_user_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserName",
                table: "twitter_user_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserNormalFollowersCount",
                table: "twitter_user_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserProfileImageUrl",
                table: "twitter_user_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserScreenName",
                table: "twitter_user_relation");

            migrationBuilder.DropColumn(
                name: "FollowingUserStatusesCount",
                table: "twitter_user_relation");
        }
    }
}
