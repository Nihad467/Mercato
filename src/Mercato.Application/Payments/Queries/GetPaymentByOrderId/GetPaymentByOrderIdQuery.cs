using MediatR;
using Mercato.Application.Payments.DTOs;

namespace Mercato.Application.Payments.Queries.GetPaymentByOrderId;

public record GetPaymentByOrderIdQuery(int OrderId) : IRequest<PaymentDto>;