using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInfluencerTagTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TweetId",
                table: "twitter_user_signal",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentCursor",
                table: "twitter_tweet_crawl_queue",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "twitter_tweet_crawl_queue",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TweetId",
                table: "twitter_user_signal");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "twitter_tweet_crawl_queue");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentCursor",
                table: "twitter_tweet_crawl_queue",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);
        }
    }
}
