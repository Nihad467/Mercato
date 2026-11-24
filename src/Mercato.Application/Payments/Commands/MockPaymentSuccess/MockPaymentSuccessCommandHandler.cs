using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Enums;

namespace Mercato.Application.Payments.Commands.MockPaymentSuccess;

public class MockPaymentSuccessCommandHandler : IRequestHandler<MockPaymentSuccessCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MockPaymentSuccessCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<string> Handle(MockPaymentSuccessCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
        var order = await _context.GetUserOrderByIdAsync(request.OrderId, userId, cancellationToken);

        if (order is null)
        {
            throw new Exception("Order not found.");
        }

        var payment = await _context.GetPaymentByOrderIdAsync(request.OrderId, cancellationToken);

        if (payment is null)
        {
            throw new Exception("Payment not found.");
        }

        if (payment.Provider != PaymentProvider.Mock)
            throw new InvalidOperationException("Bu sifariş üçün demo ödənişi istifadə edilmir.");

        payment.Status = PaymentStatus.Succeeded;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        payment.FailureReason = null;

        order.Status = OrderStatus.Paid;

        _context.UpdatePayment(payment);
        _context.UpdateOrder(order);

        await _context.SaveChangesAsync(cancellationToken);

        return "Mock payment marked as successful.";
    }
}  
