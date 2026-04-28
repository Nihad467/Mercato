namespace Mercato.Application.Admin.Dashboard.Dtos;

public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Stock { get; set; }
    public decimal Price { get; set; }
}