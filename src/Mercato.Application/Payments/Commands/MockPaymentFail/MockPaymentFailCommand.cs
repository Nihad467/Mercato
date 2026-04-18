using MediatR;

namespace Mercato.Application.Payments.Commands.MockPaymentFail;

public record MockPaymentFailCommand(int OrderId, string? Reason) : IRequest<string>;