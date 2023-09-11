using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedCrawlRelationTableAddedLinkColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EntitiesAsJson",
                table: "twitter_following_crawl_relation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfestionalAsJson",
                table: "twitter_following_crawl_relation",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntitiesAsJson",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "ProfestionalAsJson",
                table: "twitter_following_crawl_relation");
        }
    }
}
