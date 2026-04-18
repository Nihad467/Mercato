using MediatR;

namespace Mercato.Application.Payments.Commands.HandleStripeWebhook;

public record HandleStripeWebhookCommand(string Payload, string SignatureHeader) : IRequest<string>;