using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class AddedChangeLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfestionalAsJson",
                table: "twitter_following_crawl_relation",
                newName: "ProfessionalAsJson");

            migrationBuilder.CreateTable(
                name: "twitter_user_change_log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DataTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_twitter_user_change_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_crawl_category",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TwitterId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IconName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
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
                    table.PrimaryKey("PK_twitter_user_crawl_category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "twitter_user_url",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DisplayUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ExpandedUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
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
                    table.PrimaryKey("PK_twitter_user_url", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_change_log_UserId",
                table: "twitter_user_change_log",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_change_log_UserId_DataType",
                table: "twitter_user_change_log",
                columns: new[] { "UserId", "DataType" });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_crawl_category_UserId",
                table: "twitter_user_crawl_category",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_url_UserId",
                table: "twitter_user_url",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_url_UserId_Type",
                table: "twitter_user_url",
                columns: new[] { "UserId", "Type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "twitter_user_change_log");

            migrationBuilder.DropTable(
                name: "twitter_user_crawl_category");

            migrationBuilder.DropTable(
                name: "twitter_user_url");

            migrationBuilder.RenameColumn(
                name: "ProfessionalAsJson",
                table: "twitter_following_crawl_relation",
                newName: "ProfestionalAsJson");
        }
    }
}
