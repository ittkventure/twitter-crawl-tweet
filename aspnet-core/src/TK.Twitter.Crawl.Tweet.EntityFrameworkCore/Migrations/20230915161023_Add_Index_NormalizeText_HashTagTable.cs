using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexNormalizeTextHashTagTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_twitter_tweet_hash_tag_TweetId_NormalizeText",
                table: "twitter_tweet_hash_tag");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_hash_tag_NormalizeText",
                table: "twitter_tweet_hash_tag",
                column: "NormalizeText");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_twitter_tweet_hash_tag_NormalizeText",
                table: "twitter_tweet_hash_tag");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_tweet_hash_tag_TweetId_NormalizeText",
                table: "twitter_tweet_hash_tag",
                columns: new[] { "TweetId", "NormalizeText" });
        }
    }
}
