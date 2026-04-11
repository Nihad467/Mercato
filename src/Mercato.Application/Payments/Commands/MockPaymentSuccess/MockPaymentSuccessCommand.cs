using MediatR;

namespace Mercato.Application.Payments.Commands.MockPaymentSuccess;

public record MockPaymentSuccessCommand(int OrderId) : IRequest<string>;