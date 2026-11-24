using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Payments.DTOs;

namespace Mercato.Application.Payments.Queries.GetPaymentByOrderId;

public class GetPaymentByOrderIdQueryHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPaymentByOrderIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaymentDto> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
        var payment = await _context.GetPaymentByOrderIdAsync(request.OrderId, cancellationToken);

        if (payment is null || payment.Order.UserId != userId)
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
