namespace Mercato.Application.Orders.Commands.CreateOrder;

public class CreateOrderResult
{
    public int OrderId { get; set; }

    public decimal SubTotalPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalPrice { get; set; }

    public string? CouponCode { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}