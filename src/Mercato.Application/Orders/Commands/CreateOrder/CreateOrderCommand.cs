using MediatR;

namespace Mercato.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand : IRequest<int>;