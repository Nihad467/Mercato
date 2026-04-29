using MediatR;

namespace Mercato.Application.Wishlist.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommand : IRequest<bool>
{
    public int ProductId { get; set; }

    public RemoveFromWishlistCommand(int productId)
    {
        ProductId = productId;
    }
}