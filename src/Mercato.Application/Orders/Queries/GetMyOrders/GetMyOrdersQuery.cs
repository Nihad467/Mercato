using MediatR;
using Mercato.Application.Orders.DTOs;

namespace Mercato.Application.Orders.Queries.GetMyOrders;

public record GetMyOrdersQuery : IRequest<List<OrderDto>>;