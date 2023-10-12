using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class AddManualSourceTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AirTableRecordId",
                table: "twitter_user_signal",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "twitter_user_signal",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecordId",
                table: "lead_waiting_process",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "lead_waiting_process",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "air_table_manual_source",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "air_table_manual_source",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserScreenName",
                table: "air_table_manual_source",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AirTableRecordId",
                table: "twitter_user_signal");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "twitter_user_signal");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "lead_waiting_process");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "lead_waiting_process");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "air_table_manual_source");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "air_table_manual_source");

            migrationBuilder.DropColumn(
                name: "UserScreenName",
                table: "air_table_manual_source");
        }
    }
}
