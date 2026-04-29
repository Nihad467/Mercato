namespace Mercato.Application.Wishlist.Queries.GetMyWishlist;

public class GetMyWishlistResult
{
    public int WishlistItemId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int CategoryId { get; set; }

    public string? MainImageObjectKey { get; set; }

    public DateTime AddedAtUtc { get; set; }
}