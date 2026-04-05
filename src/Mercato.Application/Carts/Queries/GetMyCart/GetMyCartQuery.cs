using MediatR;
using Mercato.Application.Carts.DTOs;

namespace Mercato.Application.Carts.Queries.GetMyCart;

public record GetMyCartQuery : IRequest<CartDto>;