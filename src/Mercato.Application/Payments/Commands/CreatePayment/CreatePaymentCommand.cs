using MediatR;
using Mercato.Application.Payments.DTOs;
using Mercato.Domain.Enums;

namespace Mercato.Application.Payments.Commands.CreatePayment;

public record CreatePaymentCommand(int OrderId, PaymentProvider Provider) : IRequest<CreatePaymentResponseDto>;