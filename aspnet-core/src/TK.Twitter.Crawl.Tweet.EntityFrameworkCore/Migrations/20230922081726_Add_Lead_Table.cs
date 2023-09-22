using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "air_table_waiting_process",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchKey = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
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
                    table.PrimaryKey("PK_air_table_waiting_process", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lead",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    UserName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    UserScreenName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UserProfileUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    UserType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UserStatus = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Signals = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LastestTweetId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    LastestSponsoredDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastestSponsoredTweetUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DuplicateUrlCount = table.Column<int>(type: "integer", nullable: true),
                    TweetDescription = table.Column<string>(type: "text", nullable: true),
                    NormalizeTweetDescription = table.Column<string>(type: "text", nullable: true),
                    TweetOwnerUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    MediaMentioned = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MediaMentionedProfileUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    NumberOfSponsoredTweets = table.Column<int>(type: "integer", nullable: false),
                    HashTags = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
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
                    table.PrimaryKey("PK_lead", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lead_waiting_process",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchKey = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TweetId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
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
                    table.PrimaryKey("PK_lead_waiting_process", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_air_table_waiting_process_BatchKey_Ended",
                table: "air_table_waiting_process",
                columns: new[] { "BatchKey", "Ended" });

            migrationBuilder.CreateIndex(
                name: "IX_lead_Signals",
                table: "lead",
                column: "Signals");

            migrationBuilder.CreateIndex(
                name: "IX_lead_UserId",
                table: "lead",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_lead_UserStatus",
                table: "lead",
                column: "UserStatus");

            migrationBuilder.CreateIndex(
                name: "IX_lead_UserType",
                table: "lead",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "IX_lead_waiting_process_BatchKey_Ended",
                table: "lead_waiting_process",
                columns: new[] { "BatchKey", "Ended" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "air_table_waiting_process");

            migrationBuilder.DropTable(
                name: "lead");

            migrationBuilder.DropTable(
                name: "lead_waiting_process");
        }
    }
}
