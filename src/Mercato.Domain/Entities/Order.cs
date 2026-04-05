using Mercato.Domain.Enums;

namespace Mercato.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    public Guid UserId { get; set; } = default!;

    public decimal TotalPrice { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}