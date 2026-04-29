using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Wishlist.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RemoveFromWishlistCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var wishlistItem = await _context.GetWishlistItemAsync(
            userId,
            request.ProductId,
            cancellationToken);

        if (wishlistItem is null)
            return false;

        _context.RemoveWishlistItem(wishlistItem);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}