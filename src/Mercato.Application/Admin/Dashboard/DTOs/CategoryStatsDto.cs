namespace Mercato.Application.Admin.Dashboard.Dtos;

public class CategoryStatsDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalProductsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}