using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Entities;

namespace Mercato.Application.Wishlist.Commands.AddToWishlist;

public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AddToWishlistCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var product = await _context.GetProductByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            throw new Exception("Product not found.");

        var existingWishlistItem = await _context.GetWishlistItemAsync(
            userId,
            request.ProductId,
            cancellationToken);

        if (existingWishlistItem is not null)
            throw new Exception("Product already exists in wishlist.");

        var wishlistItem = new WishlistItem
        {
            UserId = userId,
            ProductId = request.ProductId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _context.AddWishlistItemAsync(wishlistItem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}