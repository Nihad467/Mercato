using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Enums;

namespace Mercato.Application.Payments.Commands.MockPaymentSuccess;

public class MockPaymentSuccessCommandHandler : IRequestHandler<MockPaymentSuccessCommand, string>
{
    private readonly IApplicationDbContext _context;

    public MockPaymentSuccessCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(MockPaymentSuccessCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.GetOrderByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            throw new Exception("Order not found.");
        }

        var payment = await _context.GetPaymentByOrderIdAsync(request.OrderId, cancellationToken);

        if (payment is null)
        {
            throw new Exception("Payment not found.");
        }

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