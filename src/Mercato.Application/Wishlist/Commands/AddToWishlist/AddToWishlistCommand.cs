using MediatR;

namespace Mercato.Application.Wishlist.Commands.AddToWishlist;

public class AddToWishlistCommand : IRequest<bool>
{
    public int ProductId { get; set; }

    public AddToWishlistCommand(int productId)
    {
        ProductId = productId;
    }
}