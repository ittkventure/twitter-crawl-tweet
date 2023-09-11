using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNameScreenNameTweetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScreenName",
                table: "twitter_tweet",
                newName: "UserScreenName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "twitter_tweet",
                newName: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserScreenName",
                table: "twitter_tweet",
                newName: "ScreenName");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "twitter_tweet",
                newName: "Name");
        }
    }
}
