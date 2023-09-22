using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProcessingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalizeTweetDescription",
                table: "lead");

            migrationBuilder.AddColumn<long>(
                name: "LeadId",
                table: "air_table_waiting_process",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "TweetId",
                table: "air_table_waiting_process",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserScreenName",
                table: "air_table_waiting_process",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "air_table_waiting_process");

            migrationBuilder.DropColumn(
                name: "TweetId",
                table: "air_table_waiting_process");

            migrationBuilder.DropColumn(
                name: "UserScreenName",
                table: "air_table_waiting_process");

            migrationBuilder.AddColumn<string>(
                name: "NormalizeTweetDescription",
                table: "lead",
                type: "text",
                nullable: true);
        }
    }
}
