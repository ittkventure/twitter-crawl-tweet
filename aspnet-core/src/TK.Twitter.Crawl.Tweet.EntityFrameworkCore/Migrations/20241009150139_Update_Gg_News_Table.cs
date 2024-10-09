using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGgNewsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Keyword",
                table: "google_news_record",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Keyword",
                table: "google_news_record");
        }
    }
}
