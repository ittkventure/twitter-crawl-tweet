using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class AddPaddlePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "paddle_webhook_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertId = table.Column<long>(type: "bigint", nullable: false),
                    AlertName = table.Column<string>(type: "text", nullable: true),
                    Raw = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_paddle_webhook_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "paddle_webhook_process",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlertId = table.Column<long>(type: "bigint", nullable: false),
                    AlertName = table.Column<string>(type: "text", nullable: true),
                    ProcessAttempt = table.Column<int>(type: "integer", nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    Ended = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_paddle_webhook_process", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payment_order_paddle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BalanceCurrency = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    BalanceEarnings = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    BalanceFee = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    BalanceGross = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    BalanceTax = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    CheckoutId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Country = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Coupon = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Currency = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomData = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Earnings = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Email = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    EventTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Fee = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Ip = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    MarketingConsent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PaddleOrderId = table.Column<string>(type: "text", nullable: true),
                    Passthrough = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PaymentTax = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    ProductId = table.Column<long>(type: "bigint", nullable: true),
                    ProductName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    ReceiptUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SaleGross = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    UsedPriceOverride = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    InitialPayment = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Instalments = table.Column<decimal>(type: "numeric", nullable: false),
                    NextBillDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    NextPaymentAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    PlanName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Status = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SubscriptionId = table.Column<long>(type: "bigint", nullable: false),
                    SubscriptionPaymentId = table.Column<long>(type: "bigint", nullable: false),
                    SubscriptionPlanId = table.Column<long>(type: "bigint", nullable: false),
                    UnitPrice = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_payment_order_paddle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payment_order_pay_link",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayLink = table.Column<string>(type: "text", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                    table.PrimaryKey("PK_payment_order_pay_link", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_plan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
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
                    table.PrimaryKey("PK_user_plan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_plan_cancelation_survey",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ReasonText = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Feedback = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
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
                    table.PrimaryKey("PK_user_plan_cancelation_survey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payment_order",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OrderStatusId = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    PaylinkId = table.Column<long>(type: "bigint", nullable: true),
                    PaddlePaymentInfoId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
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
                    table.PrimaryKey("PK_payment_order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_order_payment_order_paddle_PaddlePaymentInfoId",
                        column: x => x.PaddlePaymentInfoId,
                        principalTable: "payment_order_paddle",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_payment_order_payment_order_pay_link_PaylinkId",
                        column: x => x.PaylinkId,
                        principalTable: "payment_order_pay_link",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "user_plan_paddle_subscription",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<long>(type: "bigint", nullable: false),
                    IsCanceled = table.Column<bool>(type: "boolean", nullable: false),
                    CancellationEffectiveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    UserPlanEntityId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_user_plan_paddle_subscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_plan_paddle_subscription_user_plan_UserPlanEntityId",
                        column: x => x.UserPlanEntityId,
                        principalTable: "user_plan",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "user_plan_upgrade_history",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OldPlanKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NewPlanKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeAddedType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TimeAdded = table.Column<int>(type: "integer", nullable: false),
                    NewExpiredTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Reference = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UserPlanEntityId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_user_plan_upgrade_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_plan_upgrade_history_user_plan_UserPlanEntityId",
                        column: x => x.UserPlanEntityId,
                        principalTable: "user_plan",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_paddle_webhook_log_AlertId_AlertName",
                table: "paddle_webhook_log",
                columns: new[] { "AlertId", "AlertName" });

            migrationBuilder.CreateIndex(
                name: "IX_paddle_webhook_process_AlertId_AlertName_Ended_Succeeded",
                table: "paddle_webhook_process",
                columns: new[] { "AlertId", "AlertName", "Ended", "Succeeded" });

            migrationBuilder.CreateIndex(
                name: "IX_payment_order_OrderId_UserId",
                table: "payment_order",
                columns: new[] { "OrderId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_payment_order_PaddlePaymentInfoId",
                table: "payment_order",
                column: "PaddlePaymentInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_order_PaylinkId",
                table: "payment_order",
                column: "PaylinkId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_order_paddle_OrderId",
                table: "payment_order_paddle",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_order_pay_link_OrderId",
                table: "payment_order_pay_link",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_UserId",
                table: "user_plan",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_UserId_PlanKey",
                table: "user_plan",
                columns: new[] { "UserId", "PlanKey" });

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_cancelation_survey_UserId",
                table: "user_plan_cancelation_survey",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_paddle_subscription_UserId",
                table: "user_plan_paddle_subscription",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_paddle_subscription_UserPlanEntityId",
                table: "user_plan_paddle_subscription",
                column: "UserPlanEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_upgrade_history_UserId",
                table: "user_plan_upgrade_history",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_upgrade_history_UserId_Type",
                table: "user_plan_upgrade_history",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_upgrade_history_UserPlanEntityId",
                table: "user_plan_upgrade_history",
                column: "UserPlanEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "paddle_webhook_log");

            migrationBuilder.DropTable(
                name: "paddle_webhook_process");

            migrationBuilder.DropTable(
                name: "payment_order");

            migrationBuilder.DropTable(
                name: "user_plan_cancelation_survey");

            migrationBuilder.DropTable(
                name: "user_plan_paddle_subscription");

            migrationBuilder.DropTable(
                name: "user_plan_upgrade_history");

            migrationBuilder.DropTable(
                name: "payment_order_paddle");

            migrationBuilder.DropTable(
                name: "payment_order_pay_link");

            migrationBuilder.DropTable(
                name: "user_plan");
        }
    }
}
