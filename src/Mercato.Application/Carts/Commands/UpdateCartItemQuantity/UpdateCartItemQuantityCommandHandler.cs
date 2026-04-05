using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Carts.Commands.UpdateCartItemQuantity;

public class UpdateCartItemQuantityCommandHandler : IRequestHandler<UpdateCartItemQuantityCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCartItemQuantityCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var cartItem = await _context.GetCartItemByIdAsync(request.CartItemId, userId, cancellationToken);
        if (cartItem is null)
            throw new Exception("Cart item not found.");

        if (cartItem.Product.Stock < request.Quantity)
            throw new Exception("Not enough stock.");

        cartItem.Quantity = request.Quantity;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}