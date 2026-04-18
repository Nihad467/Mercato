using Mercato.Application.Common.Interfaces;
using Mercato.Application.Payments.DTOs;
using Mercato.Domain.Entities;
using Mercato.Domain.Enums;

namespace Mercato.Infrastructure.Services.Payments;

public class MockPaymentService : IPaymentService
{
    public PaymentProvider Provider => PaymentProvider.Mock;

    public Task<CreatePaymentResponseDto> CreatePaymentAsync(Order order, CancellationToken cancellationToken)
    {
        var externalPaymentId = $"mock_{Guid.NewGuid():N}";

        var response = new CreatePaymentResponseDto
        {
            PaymentId = Guid.Empty,
            OrderId = order.Id,
            Provider = PaymentProvider.Mock,
            Status = PaymentStatus.Pending,
            ExternalPaymentId = externalPaymentId,
            CheckoutUrl = $"mock://payment/{order.Id}",
            ClientSecret = null,
            FailureReason = null
        };

        return Task.FromResult(response);
    }
    public Task<PaymentWebhookResultDto> HandleWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Mock payment provider does not support webhook handling.");
    }
}