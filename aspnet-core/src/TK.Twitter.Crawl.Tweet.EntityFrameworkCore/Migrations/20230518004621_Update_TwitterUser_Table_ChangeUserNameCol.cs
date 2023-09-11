using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTwitterUserTableChangeUserNameCol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "twitter_user",
                newName: "ScreenName");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "twitter_following_crawl_user",
                newName: "ScreenName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScreenName",
                table: "twitter_user",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "ScreenName",
                table: "twitter_following_crawl_user",
                newName: "Username");
        }
    }
}
