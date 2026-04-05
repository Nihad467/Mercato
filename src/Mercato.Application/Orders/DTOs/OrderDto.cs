namespace Mercato.Application.Orders.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}