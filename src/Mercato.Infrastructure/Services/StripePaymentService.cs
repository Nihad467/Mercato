using Mercato.Application.Common.Interfaces;
using Mercato.Application.Options;
using Mercato.Application.Payments.DTOs;
using Mercato.Domain.Entities;
using Mercato.Domain.Enums;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Mercato.Infrastructure.Services.Payments;

public class StripePaymentService : IPaymentService
{
    private readonly StripeOptions _stripeOptions;

    public StripePaymentService(IOptions<StripeOptions> stripeOptions)
    {
        _stripeOptions = stripeOptions.Value;
        StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
    }

    public PaymentProvider Provider => PaymentProvider.Stripe;

    public async Task<CreatePaymentResponseDto> CreatePaymentAsync(Order order, CancellationToken cancellationToken)
    {
        var service = new SessionService();

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = _stripeOptions.SuccessUrl,
            CancelUrl = _stripeOptions.CancelUrl,
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
                        Currency = _stripeOptions.Currency.ToLower(),
                        UnitAmount = (long)(order.TotalPrice * 100),
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
            ExternalPaymentId = session.Id,
            CheckoutUrl = session.Url,
            ClientSecret = session.ClientSecret,
            FailureReason = null
        };
    }

    public Task<PaymentWebhookResultDto> HandleWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken)
    {
        var stripeEvent = EventUtility.ConstructEvent(
            payload,
            signatureHeader,
            _stripeOptions.WebhookSecret);

        if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = stripeEvent.Data.Object as Session;

            if (session is null)
            {
                throw new Exception("Invalid Stripe session payload.");
            }

            var orderIdString = session.Metadata["orderId"];
            var orderId = int.Parse(orderIdString);

            var result = new PaymentWebhookResultDto
            {
                IsSuccess = true,
                OrderId = orderId,
                ExternalPaymentId = session.Id,
                PaymentStatus = PaymentStatus.Succeeded,
                FailureReason = null
            };

            return Task.FromResult(result);
        }

        if (stripeEvent.Type == "checkout.session.expired")
        {
            var session = stripeEvent.Data.Object as Session;

            if (session is null)
            {
                throw new Exception("Invalid Stripe session payload.");
            }

            var orderIdString = session.Metadata["orderId"];
            var orderId = int.Parse(orderIdString);

            var result = new PaymentWebhookResultDto
            {
                IsSuccess = false,
                OrderId = orderId,
                ExternalPaymentId = session.Id,
                PaymentStatus = PaymentStatus.Failed,
                FailureReason = "Checkout session expired."
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
}