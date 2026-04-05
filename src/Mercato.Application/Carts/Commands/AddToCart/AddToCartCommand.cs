using MediatR;

namespace Mercato.Application.Carts.Commands.AddToCart;

public record AddToCartCommand(int ProductId, int Quantity) : IRequest<Unit>;