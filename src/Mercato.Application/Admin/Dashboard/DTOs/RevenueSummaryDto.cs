namespace Mercato.Application.Admin.Dashboard.Dtos;

public class RevenueSummaryDto
{
    public decimal TodayRevenue { get; set; }
    public decimal WeeklyRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
}