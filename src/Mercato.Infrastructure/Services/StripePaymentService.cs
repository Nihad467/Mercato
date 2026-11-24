using Mercato.Application.Common.Interfaces;
using Mercato.Application.Options;
using Mercato.Application.Payments.DTOs;
using Mercato.Domain.Entities;
using Mercato.Domain.Enums;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Globalization;

namespace Mercato.Infrastructure.Services.Payments;

public class StripePaymentService : IPaymentService
{
    private readonly StripeOptions _stripeOptions;

    public StripePaymentService(IOptions<StripeOptions> stripeOptions)
    {
        _stripeOptions = stripeOptions.Value;
    }

    public PaymentProvider Provider => PaymentProvider.Stripe;

    public async Task<CreatePaymentResponseDto> CreatePaymentAsync(Order order, CancellationToken cancellationToken)
    {
        var secretKey = _stripeOptions.SecretKey?.Trim();
        if (!_stripeOptions.HasValidSecretKey)
        {
            throw new InvalidOperationException("Stripe Secret Key konfiqurasiya edilməyib.");
        }

        var currency = (_stripeOptions.Currency ?? "azn").Trim().ToLowerInvariant();
        if (currency.Length != 3 || currency.Any(character => character < 'a' || character > 'z'))
        {
            throw new InvalidOperationException("Stripe valyutası üç kiçik hərfdən ibarət olmalıdır.");
        }

        var amountInMinorUnits = decimal.Round(order.TotalPrice * 100m, 0, MidpointRounding.AwayFromZero);
        if (amountInMinorUnits < 1 || amountInMinorUnits > long.MaxValue)
        {
            throw new InvalidOperationException("Stripe ödəniş məbləği etibarlı deyil.");
        }

        StripeConfiguration.ApiKey = secretKey;
        var service = new SessionService();

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = BuildReturnUrl(_stripeOptions.SuccessUrl, order.Id),
            CancelUrl = BuildReturnUrl(_stripeOptions.CancelUrl, order.Id),
            ClientReferenceId = order.Id.ToString(),
            Metadata = new Dictionary<string, string>
            {
                { "orderId", order.Id.ToString() }
            },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency,
                        UnitAmount = (long)amountInMinorUnits,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Mercato Order #{order.Id}"
                        }
                    }
                }
            }
        };

        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return new CreatePaymentResponseDto
        {
            PaymentId = Guid.Empty,
            OrderId = order.Id,
            Provider = PaymentProvider.Stripe,
            Status = PaymentStatus.Pending,
            Currency = currency,
            ExternalPaymentId = session.Id,
            CheckoutUrl = session.Url,
            ClientSecret = session.ClientSecret,
            FailureReason = null
        };
    }

    public Task<PaymentWebhookResultDto> HandleWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken)
    {
        if (!_stripeOptions.HasValidWebhookSecret)
        {
            throw new InvalidOperationException("Stripe Webhook Signing Secret konfiqurasiya edilməyib.");
        }

        var stripeEvent = EventUtility.ConstructEvent(
            payload,
            signatureHeader,
            _stripeOptions.WebhookSecret);

        if (stripeEvent.Type is "checkout.session.completed"
            or "checkout.session.async_payment_succeeded"
            or "checkout.session.async_payment_failed"
            or "checkout.session.expired")
        {
            var session = stripeEvent.Data.Object as Session;

            if (session is null)
            {
                throw new Exception("Invalid Stripe session payload.");
            }

            if (!session.Metadata.TryGetValue("orderId", out var orderIdString)
                || !int.TryParse(orderIdString, out var orderId))
                throw new InvalidOperationException("Stripe sessiyasında etibarlı sifariş nömrəsi yoxdur.");

            var succeeded = stripeEvent.Type == "checkout.session.async_payment_succeeded"
                || (stripeEvent.Type == "checkout.session.completed" && session.PaymentStatus == "paid");
            var failed = stripeEvent.Type is "checkout.session.async_payment_failed" or "checkout.session.expired";

            var result = new PaymentWebhookResultDto
            {
                IsSuccess = succeeded,
                OrderId = orderId,
                ExternalPaymentId = session.Id,
                PaymentStatus = succeeded
                    ? PaymentStatus.Succeeded
                    : failed ? PaymentStatus.Failed : PaymentStatus.Pending,
                FailureReason = failed ? $"Stripe Checkout {stripeEvent.Type}." : null
            };

            return Task.FromResult(result);
        }

        return Task.FromResult(new PaymentWebhookResultDto
        {
            IsSuccess = false,
            OrderId = 0,
            ExternalPaymentId = string.Empty,
            PaymentStatus = PaymentStatus.Pending,
            FailureReason = $"Unhandled Stripe event type: {stripeEvent.Type}"
        });
    }

    private static string BuildReturnUrl(string? template, int orderId)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            throw new InvalidOperationException("Stripe success/cancel URL konfiqurasiya edilməyib.");
        }

        var url = template.Replace("{ORDER_ID}", orderId.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
        if (!Uri.TryCreate(url.Replace("{CHECKOUT_SESSION_ID}", "session", StringComparison.Ordinal), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && !(uri.Scheme == Uri.UriSchemeHttp && uri.Host == "localhost")))
        {
            throw new InvalidOperationException("Stripe return URL etibarlı HTTPS və ya localhost ünvanı olmalıdır.");
        }

        return url;
    }

}
