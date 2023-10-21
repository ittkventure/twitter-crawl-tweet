using System;
using System.Globalization;
using System.Web;
using TK.Paddle.Domain.Dto;

namespace TK.Paddle.Domain
{
    public class PaddleDataAdapter
    {
        private static NumberFormatInfo DecimalNFI = new NumberFormatInfo()
        {
            NegativeSign = "-",
            CurrencyDecimalSeparator = ".",
            CurrencyGroupSeparator = ",",
            CurrencySymbol = "$"
        };

        public static PaddleWebhookSubscriptionPaymentSuccessInput GetSubscriptionPaymentSuccessInput(string raw)
        {
            var input = new PaddleWebhookSubscriptionPaymentSuccessInput()
            {
                Raw = raw
            };

            var pairs = raw.Split('&');
            foreach (var p in pairs)
            {
                var kv = p.Split('=');
                if (kv.Length != 2)
                {
                    continue;
                }

                var key = kv[0];
                var value = kv[1];

                if (value.IsEmpty())
                {
                    continue;
                }

                value = HttpUtility.UrlDecode(value);

                switch (key)
                {
                    case "alert_id":
                        input.AlertId = long.Parse(value);
                        break;
                    case "alert_name":
                        input.AlertName = value;
                        break;
                    default:
                        break;
                }

                if (key == "balance_currency")
                {
                    input.BalanceCurrency = value;
                }
                else if (key == "balance_earnings")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.BalanceEarnings = v;
                    }
                }
                else if (key == "balance_fee")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.BalanceFee = v;
                    }
                }
                else if (key == "balance_gross")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.BalanceGross = v;
                    }
                }
                else if (key == "balance_tax")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.BalanceTax = v;
                    }
                }
                else if (key == "checkout_id")
                {
                    input.CheckoutId = value;
                }
                else if (key == "country")
                {
                    input.Country = value;
                }
                else if (key == "coupon")
                {
                    input.Coupon = value;
                }
                else if (key == "currency")
                {
                    input.Currency = value;
                }
                else if (key == "custom_data")
                {
                    input.CustomData = value;
                }
                else if (key == "customer_name")
                {
                    input.CustomerName = value;
                }
                else if (key == "earnings")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.Earnings = v;
                    }
                }
                else if (key == "email")
                {
                    input.Email = value;
                }
                else if (key == "fee")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.Fee = v;
                    }
                }
                else if (key == "initial_payment")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.InitialPayment = v;
                    }
                }
                else if (key == "instalments")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.Instalments = v;
                    }
                }
                else if (key == "next_bill_date")
                {
                    if (DateTime.TryParse(value, out var v))
                    {
                        input.NextBillDate = v;
                    }
                }
                else if (key == "next_payment_amount")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.NextPaymentAmount = v;
                    }
                }
                else if (key == "plan_name")
                {
                    input.PlanName = value;
                }
                else if (key == "ip")
                {
                    input.Ip = value;
                }
                else if (key == "marketing_consent")
                {
                    input.MarketingConsent = value;
                }
                else if (key == "order_id")
                {
                    input.OrderId = value;
                }
                else if (key == "passthrough")
                {
                    input.Passthrough = value;
                }
                else if (key == "payment_method")
                {
                    input.PaymentMethod = value;
                }
                else if (key == "payment_tax")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.PaymentTax = v;
                    }
                }
                else if (key == "product_id")
                {
                    if (long.TryParse(value, out var v))
                    {
                        input.ProductId = v;
                    }
                }
                else if (key == "product_name")
                {
                    input.ProductName = value;
                }
                else if (key == "quantity")
                {
                    if (int.TryParse(value, out var v))
                    {
                        input.Quantity = v;
                    }
                }
                else if (key == "receipt_url")
                {
                    input.ReceiptUrl = value;
                }
                else if (key == "status")
                {
                    input.Status = value;
                }
                else if (key == "subscription_id")
                {
                    if (long.TryParse(value, out var v))
                    {
                        input.SubscriptionId = v;
                    }
                }
                else if (key == "subscription_payment_id")
                {
                    if (long.TryParse(value, out var v))
                    {
                        input.SubscriptionPaymentId = v;
                    }
                }
                else if (key == "subscription_plan_id")
                {
                    if (long.TryParse(value, out var v))
                    {
                        input.SubscriptionPlanId = v;
                    }
                }
                else if (key == "unit_price")
                {
                    input.UnitPrice = value;
                }
                else if (key == "user_id")
                {
                    input.UserId = value;
                }
                else if (key == "sale_gross")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.SaleGross = v;
                    }
                }
                else if (key == "used_price_override")
                {
                    if (decimal.TryParse(value, DecimalNFI, out var v))
                    {
                        input.UsedPriceOverride = v;
                    }
                }
                else if (key == "event_time")
                {
                    if (DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var v))
                    {
                        input.EventTime = v;
                    }
                    else if (DateTime.TryParseExact(value, "yyyy-MM-ddHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var v1))
                    {
                        input.EventTime = v1;
                    };
                }
            }

            return input;
        }

        public static PaddleWebhookSubscriptionCanceledInput GetSubscriptionCanceledInput(string raw)
        {
            var input = new PaddleWebhookSubscriptionCanceledInput()
            {
                Raw = raw
            };

            var pairs = raw.Split('&');
            foreach (var p in pairs)
            {
                var kv = p.Split('=');
                if (kv.Length != 2)
                {
                    continue;
                }

                var key = kv[0];
                var value = kv[1];

                if (value.IsEmpty())
                {
                    continue;
                }

                value = HttpUtility.UrlDecode(value);

                switch (key)
                {
                    case "alert_id":
                        input.AlertId = long.Parse(value);
                        break;
                    case "alert_name":
                        input.AlertName = value;
                        break;
                    default:
                        break;
                }

                if (key == "cancellation_effective_date")
                {
                    if (DateTime.TryParse(value, out var v))
                    {
                        input.CancellationEffectiveDate = v;
                    }
                }
                else if (key == "checkout_id")
                {
                    input.CheckoutId = value;
                }
                else if (key == "currency")
                {
                    input.Currency = value;
                }
                else if (key == "custom_data")
                {
                    input.CustomData = value;
                }
                else if (key == "email")
                {
                    input.Email = value;
                }
                else if (key == "event_time")
                {
                    if (DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var v))
                    {
                        input.EventTime = v;
                    }
                    else if (DateTime.TryParseExact(value, "yyyy-MM-ddHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var v1))
                    {
                        input.EventTime = v1;
                    };
                }
                else if (key == "passthrough")
                {
                    input.Passthrough = value;
                }
                else if (key == "quantity")
                {
                    if (int.TryParse(value, out var v))
                    {
                        input.Quantity = v;
                    }
                }
                else if (key == "status")
                {
                    input.Status = value;
                }
                else if (key == "subscription_id")
                {
                    if (long.TryParse(value, out var v))
                    {
                        input.SubscriptionId = v;
                    }
                }
                else if (key == "subscription_plan_id")
                {
                    if (long.TryParse(value, out var v))
                    {
                        input.SubscriptionPlanId = v;
                    }
                }
                else if (key == "unit_price")
                {
                    input.UnitPrice = value;
                }

                else if (key == "user_id")
                {
                    input.UserId = value;
                }
            }

            return input;
        }
    }
}