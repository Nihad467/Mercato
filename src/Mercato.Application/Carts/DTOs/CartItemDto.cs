namespace Mercato.Application.Carts.DTOs;

public class CartItemDto
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? MainImageObjectKey { get; set; }
}