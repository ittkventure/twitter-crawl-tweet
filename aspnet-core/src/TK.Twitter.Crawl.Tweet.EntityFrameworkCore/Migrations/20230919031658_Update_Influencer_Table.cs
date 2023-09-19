using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInfluencerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "twitter_influencer",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "twitter_influencer");
        }
    }
}
