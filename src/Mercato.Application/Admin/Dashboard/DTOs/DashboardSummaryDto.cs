namespace Mercato.Application.Admin.Dashboard.Dtos;

public class DashboardSummaryDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
    public int LowStockCount { get; set; }
    public int PendingOrders { get; set; }
}