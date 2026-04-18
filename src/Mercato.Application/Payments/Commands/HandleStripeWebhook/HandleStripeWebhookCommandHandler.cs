using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Enums;

namespace Mercato.Application.Payments.Commands.HandleStripeWebhook;

public class HandleStripeWebhookCommandHandler : IRequestHandler<HandleStripeWebhookCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentServiceFactory _paymentServiceFactory;

    public HandleStripeWebhookCommandHandler(
        IApplicationDbContext context,
        IPaymentServiceFactory paymentServiceFactory)
    {
        _context = context;
        _paymentServiceFactory = paymentServiceFactory;
    }

    public async Task<string> Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        var stripeService = _paymentServiceFactory.GetService(PaymentProvider.Stripe);

        var webhookResult = await stripeService.HandleWebhookAsync(
            request.Payload,
            request.SignatureHeader,
            cancellationToken);

        if (webhookResult.OrderId == 0 || string.IsNullOrWhiteSpace(webhookResult.ExternalPaymentId))
        {
            return "Webhook received but ignored.";
        }

        var payment = await _context.GetPaymentByExternalPaymentIdAsync(
            webhookResult.ExternalPaymentId,
            cancellationToken);

        if (payment is null)
        {
            throw new Exception("Payment not found for Stripe webhook.");
        }

        var order = await _context.GetOrderByIdAsync(webhookResult.OrderId, cancellationToken);

        if (order is null)
        {
            throw new Exception("Order not found for Stripe webhook.");
        }

        if (payment.Status == PaymentStatus.Succeeded && order.Status == OrderStatus.Paid)
        {
            return "Webhook already processed.";
        }

        payment.Status = webhookResult.PaymentStatus;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        payment.FailureReason = webhookResult.FailureReason;

        if (webhookResult.PaymentStatus == PaymentStatus.Succeeded)
        {
            order.Status = OrderStatus.Paid;
        }
        else if (webhookResult.PaymentStatus == PaymentStatus.Failed)
        {
            order.Status = OrderStatus.PaymentFailed;
        }

        _context.UpdatePayment(payment);
        _context.UpdateOrder(order);

        await _context.SaveChangesAsync(cancellationToken);

        return "Webhook processed successfully.";
    }
}