using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "payment_order",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "payment_order",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "email_log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    To = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Bcc = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Cc = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Body = table.Column<string>(type: "text", nullable: true),
                    IsBodyHtml = table.Column<bool>(type: "boolean", nullable: false),
                    Subject = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ProcessAttempt = table.Column<int>(type: "integer", nullable: false),
                    Ended = table.Column<bool>(type: "boolean", nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_log", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_order_OrderId_Email",
                table: "payment_order",
                columns: new[] { "OrderId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_email_log_Ended_Succeeded",
                table: "email_log",
                columns: new[] { "Ended", "Succeeded" });

            migrationBuilder.CreateIndex(
                name: "IX_email_log_To",
                table: "email_log",
                column: "To");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_log");

            migrationBuilder.DropIndex(
                name: "IX_payment_order_OrderId_Email",
                table: "payment_order");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "payment_order");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "payment_order",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
