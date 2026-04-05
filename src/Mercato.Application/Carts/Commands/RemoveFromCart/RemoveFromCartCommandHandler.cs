using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Carts.Commands.RemoveFromCart;

public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RemoveFromCartCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var cartItem = await _context.GetCartItemByIdAsync(request.CartItemId, userId, cancellationToken);
        if (cartItem is null)
            throw new Exception("Cart item not found.");

        _context.RemoveCartItem(cartItem);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}