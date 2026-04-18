using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Payments.DTOs;

namespace Mercato.Application.Payments.Queries.GetPaymentByOrderId;

public class GetPaymentByOrderIdQueryHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentByOrderIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _context.GetPaymentByOrderIdAsync(request.OrderId, cancellationToken);

        if (payment is null)
        {
            throw new Exception("Payment not found.");
        }

        return new PaymentDto
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            Provider = payment.Provider,
            Status = payment.Status,
            Amount = payment.Amount,
            Currency = payment.Currency,
            ExternalPaymentId = payment.ExternalPaymentId,
            CheckoutUrl = payment.CheckoutUrl,
            FailureReason = payment.FailureReason,
            CreatedAtUtc = payment.CreatedAtUtc,
            UpdatedAtUtc = payment.UpdatedAtUtc
        };
    }
}