using MediatR;

namespace Mercato.Application.Carts.Commands.UpdateCartItemQuantity;

public record UpdateCartItemQuantityCommand(int CartItemId, int Quantity) : IRequest<Unit>;