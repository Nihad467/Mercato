namespace Mercato.Domain.Entities;

public class CartItem
{
    public int Id { get; set; }

    public Guid UserId { get; set; } = default!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public int Quantity { get; set; }
}