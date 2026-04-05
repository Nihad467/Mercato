using MediatR;

namespace Mercato.Application.Carts.Commands.RemoveFromCart;

public record RemoveFromCartCommand(int CartItemId) : IRequest<Unit>;