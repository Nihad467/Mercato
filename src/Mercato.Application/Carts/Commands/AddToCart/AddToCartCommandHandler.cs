using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Entities;

namespace Mercato.Application.Carts.Commands.AddToCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AddToCartCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var product = await _context.GetProductByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            throw new Exception("Product not found.");

        if (product.Stock < request.Quantity)
            throw new Exception("Not enough stock.");

        var existingCartItem = await _context.GetCartItemAsync(userId, request.ProductId, cancellationToken);

        if (existingCartItem is not null)
        {
            var newQuantity = existingCartItem.Quantity + request.Quantity;

            if (product.Stock < newQuantity)
                throw new Exception("Not enough stock.");

            existingCartItem.Quantity = newQuantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            await _context.AddCartItemAsync(cartItem, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}