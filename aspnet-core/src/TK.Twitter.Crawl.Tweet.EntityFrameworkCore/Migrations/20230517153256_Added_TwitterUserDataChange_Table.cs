using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class AddedTwitterUserDataChangeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_twitter_following_crawl_relation_twitter_following_crawl_ba~",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropForeignKey(
                name: "FK_twitter_following_crawl_user_twitter_following_crawl_batch_~",
                table: "twitter_following_crawl_user");

            migrationBuilder.DropIndex(
                name: "IX_twitter_following_crawl_user_TwitterFollowingCrawlBatchEnti~",
                table: "twitter_following_crawl_user");

            migrationBuilder.DropIndex(
                name: "IX_twitter_following_crawl_relation_TwitterFollowingCrawlBatch~",
                table: "twitter_following_crawl_relation");

            migrationBuilder.DropColumn(
                name: "TwitterFollowingCrawlBatchEntityId",
                table: "twitter_following_crawl_user");

            migrationBuilder.DropColumn(
                name: "TwitterFollowingCrawlBatchEntityId",
                table: "twitter_following_crawl_relation");

            migrationBuilder.CreateTable(
                name: "twitter_user_data_change",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FieldName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DataType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ScanTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FromTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ToTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OriginalData = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_twitter_user_data_change", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_data_change_UserId",
                table: "twitter_user_data_change",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_user_data_change_UserId_FieldName_IsCurrent",
                table: "twitter_user_data_change",
                columns: new[] { "UserId", "FieldName", "IsCurrent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "twitter_user_data_change");

            migrationBuilder.AddColumn<long>(
                name: "TwitterFollowingCrawlBatchEntityId",
                table: "twitter_following_crawl_user",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TwitterFollowingCrawlBatchEntityId",
                table: "twitter_following_crawl_relation",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_crawl_user_TwitterFollowingCrawlBatchEnti~",
                table: "twitter_following_crawl_user",
                column: "TwitterFollowingCrawlBatchEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_twitter_following_crawl_relation_TwitterFollowingCrawlBatch~",
                table: "twitter_following_crawl_relation",
                column: "TwitterFollowingCrawlBatchEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_twitter_following_crawl_relation_twitter_following_crawl_ba~",
                table: "twitter_following_crawl_relation",
                column: "TwitterFollowingCrawlBatchEntityId",
                principalTable: "twitter_following_crawl_batch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_twitter_following_crawl_user_twitter_following_crawl_batch_~",
                table: "twitter_following_crawl_user",
                column: "TwitterFollowingCrawlBatchEntityId",
                principalTable: "twitter_following_crawl_batch",
                principalColumn: "Id");
        }
    }
}
