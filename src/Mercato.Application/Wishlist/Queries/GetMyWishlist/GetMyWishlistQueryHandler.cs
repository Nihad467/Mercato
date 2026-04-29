using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Wishlist.Queries.GetMyWishlist;

public class GetMyWishlistQueryHandler : IRequestHandler<GetMyWishlistQuery, List<GetMyWishlistResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyWishlistQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<GetMyWishlistResult>> Handle(
        GetMyWishlistQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var wishlistItems = await _context.GetUserWishlistItemsAsync(userId, cancellationToken);

        return wishlistItems.Select(x => new GetMyWishlistResult
        {
            WishlistItemId = x.Id,
            ProductId = x.ProductId,
            ProductName = x.Product?.Name ?? string.Empty,
            Description = x.Product?.Description,
            Price = x.Product?.Price ?? 0,
            Stock = x.Product?.Stock ?? 0,
            CategoryId = x.Product?.CategoryId ?? 0,
            MainImageObjectKey = x.Product?
                .Images?
                .Where(i => i.IsMain)
                .Select(i => i.ObjectKey)
                .FirstOrDefault(),
            AddedAtUtc = x.CreatedAtUtc
        }).ToList();
    }
}