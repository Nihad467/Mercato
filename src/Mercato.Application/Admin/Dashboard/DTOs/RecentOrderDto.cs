namespace Mercato.Application.Admin.Dashboard.Dtos;

public class RecentOrderDto
{
    public int OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}