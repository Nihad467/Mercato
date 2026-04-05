using MediatR;
using Mercato.Domain.Enums;

namespace Mercato.Application.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(int OrderId, OrderStatus Status) : IRequest<Unit>;