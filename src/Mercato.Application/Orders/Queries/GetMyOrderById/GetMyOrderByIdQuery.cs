using MediatR;
using Mercato.Application.Orders.DTOs;

namespace Mercato.Application.Orders.Queries.GetMyOrderById;

public record GetMyOrderByIdQuery(int Id) : IRequest<OrderDto>;