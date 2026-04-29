using MediatR;

namespace Mercato.Application.Wishlist.Queries.GetMyWishlist;

public class GetMyWishlistQuery : IRequest<List<GetMyWishlistResult>>
{
}