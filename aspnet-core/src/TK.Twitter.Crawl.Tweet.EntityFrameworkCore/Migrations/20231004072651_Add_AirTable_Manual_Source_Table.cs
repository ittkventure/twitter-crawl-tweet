using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class AddAirTableManualSourceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "air_table_manual_source",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecordId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ProjectTwitter = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Signals = table.Column<string>(type: "text", nullable: true),
                    LastestSignalFrom = table.Column<string>(type: "text", nullable: true),
                    LastestSignalTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastestSignalDescription = table.Column<string>(type: "text", nullable: true),
                    LastestSignalUrl = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_air_table_manual_source", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "air_table_manual_source_waiting_process",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectTwitter = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RecordId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Action = table.Column<string>(type: "text", nullable: true),
                    Ended = table.Column<bool>(type: "boolean", nullable: false),
                    Succeed = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_air_table_manual_source_waiting_process", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_air_table_manual_source_RecordId",
                table: "air_table_manual_source",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_air_table_manual_source_waiting_process_Ended",
                table: "air_table_manual_source_waiting_process",
                column: "Ended");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "air_table_manual_source");

            migrationBuilder.DropTable(
                name: "air_table_manual_source_waiting_process");
        }
    }
}
