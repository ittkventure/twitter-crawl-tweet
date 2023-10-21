using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TK.Twitter.Crawl.Entity;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace TK.Twitter.Crawl.EntityFrameworkCore;

public static class PaymentManagmentDbContextModelBuilderExtensions
{
    public static void ConfigurePaymentManagment([NotNull] this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<PaymentOrderEntity>(b =>
        {
            b.ToTable("payment_order");
            b.ConfigureByConvention();

            b.Property(x => x.Email).HasMaxLength(256);

            b.HasIndex(x => new { x.OrderId, x.UserId });
            b.HasIndex(x => new { x.OrderId, x.Email });
        });

        builder.Entity<PaymentOrderPayLinkEntity>(b =>
        {
            b.ToTable("payment_order_pay_link");
            b.ConfigureByConvention();

            b.HasIndex(x => new { x.OrderId });
        });

        builder.Entity<PaymentOrderPaddleEntity>(b =>
        {
            b.ToTable("payment_order_paddle");
            b.ConfigureByConvention();

            b.Property(x => x.BalanceCurrency).HasMaxLength(128);
            b.Property(x => x.BalanceCurrency).HasMaxLength(64);
            b.Property(x => x.BalanceEarnings).HasPrecision(18, 6);
            b.Property(x => x.BalanceFee).HasPrecision(18, 6);
            b.Property(x => x.BalanceGross).HasPrecision(18, 6);
            b.Property(x => x.BalanceTax).HasPrecision(18, 6);
            b.Property(x => x.CheckoutId).HasMaxLength(64);
            b.Property(x => x.Country).HasMaxLength(64);
            b.Property(x => x.Coupon).HasMaxLength(64);
            b.Property(x => x.Currency).HasMaxLength(64);
            b.Property(x => x.CustomData).HasMaxLength(1000);
            b.Property(x => x.CustomerName).HasMaxLength(512);
            b.Property(x => x.Earnings).HasPrecision(18, 6);
            b.Property(x => x.Email).HasMaxLength(512);
            b.Property(x => x.Fee).HasPrecision(18, 6);
            b.Property(x => x.Ip).HasMaxLength(32);
            b.Property(x => x.MarketingConsent).HasMaxLength(1000);
            b.Property(x => x.Passthrough).HasMaxLength(1000);
            b.Property(x => x.PaymentMethod).HasMaxLength(64);
            b.Property(x => x.PaymentTax).HasPrecision(18, 6);
            b.Property(x => x.ProductId);
            b.Property(x => x.ProductName).HasMaxLength(256);
            b.Property(x => x.ReceiptUrl).HasMaxLength(1000);
            b.Property(x => x.SaleGross).HasPrecision(18, 6);
            b.Property(x => x.UsedPriceOverride).HasPrecision(18, 6);
            b.Property(x => x.InitialPayment).HasPrecision(18, 6);
            b.Property(x => x.NextPaymentAmount).HasPrecision(18, 6);
            b.Property(x => x.PlanName).HasMaxLength(256);
            b.Property(x => x.Status).HasMaxLength(128);
            b.Property(x => x.UnitPrice).HasMaxLength(128);
            b.Property(x => x.UserId).HasMaxLength(128);

            b.HasIndex(x => new { x.OrderId });
        });
    }

}