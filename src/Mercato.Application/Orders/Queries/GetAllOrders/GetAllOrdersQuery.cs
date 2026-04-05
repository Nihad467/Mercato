using MediatR;
using Mercato.Application.Orders.DTOs;

namespace Mercato.Application.Orders.Queries.GetAllOrders;

public record GetAllOrdersQuery : IRequest<List<OrderDto>>;