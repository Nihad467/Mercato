using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Payments.DTOs;
using Mercato.Domain.Entities;
using Mercato.Domain.Enums;

namespace Mercato.Application.Payments.Commands.CreatePayment;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, CreatePaymentResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentServiceFactory _paymentServiceFactory;

    public CreatePaymentCommandHandler(
        IApplicationDbContext context,
        IPaymentServiceFactory paymentServiceFactory)
    {
        _context = context;
        _paymentServiceFactory = paymentServiceFactory;
    }

    public async Task<CreatePaymentResponseDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.GetOrderByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            throw new Exception("Order not found.");
        }

        if (order.Status == OrderStatus.Paid)
        {
            throw new Exception("This order is already paid.");
        }

        var existingPendingPayment = await _context.GetPaymentByOrderIdAsync(request.OrderId, cancellationToken);

        if (existingPendingPayment is not null && existingPendingPayment.Status == PaymentStatus.Pending)
        {
            throw new Exception("There is already a pending payment for this order.");
        }

        var paymentService = _paymentServiceFactory.GetService(request.Provider);
        var paymentResponse = await paymentService.CreatePaymentAsync(order, cancellationToken);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Provider = request.Provider,
            Amount = order.TotalPrice,
            Currency = "usd",
            Status = PaymentStatus.Pending,
            ExternalPaymentId = paymentResponse.ExternalPaymentId,
            CheckoutUrl = paymentResponse.CheckoutUrl,
            ClientSecret = paymentResponse.ClientSecret,
            FailureReason = null,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _context.AddPaymentAsync(payment, cancellationToken);

        order.Status = OrderStatus.PaymentPending;
        _context.UpdateOrder(order);

        await _context.SaveChangesAsync(cancellationToken);

        paymentResponse.PaymentId = payment.Id;
        paymentResponse.Status = payment.Status;

        return paymentResponse;
    }
}