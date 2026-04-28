using Mercato.Application.Admin.Dashboard.Dtos;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Infrastructure.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private const int LowStockThreshold = 5;

    private readonly IApplicationDbContext _context;

    public AdminDashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var totalOrders = await _context.GetTotalOrdersCountAsync(cancellationToken);
        var totalRevenue = await _context.GetTotalRevenueAsync(cancellationToken);
        var totalProducts = await _context.GetTotalProductsCountAsync(cancellationToken);
        var totalUsers = await _context.GetTotalUsersCountAsync(cancellationToken);
        var lowStockCount = await _context.GetLowStockProductsCountAsync(
            LowStockThreshold,
            cancellationToken);
        var pendingOrders = await _context.GetPendingOrdersCountAsync(cancellationToken);

        return new DashboardSummaryDto
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            TotalProducts = totalProducts,
            TotalUsers = totalUsers,
            LowStockCount = lowStockCount,
            PendingOrders = pendingOrders
        };

    }
    public async Task<List<TopProductDto>> GetTopProductsAsync(
    int take,
    CancellationToken cancellationToken = default)
    {
        if (take <= 0)
            take = 5;

        if (take > 50)
            take = 50;

        return await _context.GetTopProductsAsync(take, cancellationToken);
    }
    public async Task<List<LowStockProductDto>> GetLowStockProductsAsync(
    int threshold,
    CancellationToken cancellationToken = default)
    {
        if (threshold <= 0)
            threshold = 5;

        if (threshold > 100)
            threshold = 100;

        return await _context.GetLowStockProductsAsync(
            threshold,
            cancellationToken);
    }
    public async Task<List<RecentOrderDto>> GetRecentOrdersAsync(
    int take,
    CancellationToken cancellationToken = default)
    {
        if (take <= 0)
            take = 10;

        if (take > 100)
            take = 100;

        return await _context.GetRecentOrdersAsync(
            take,
            cancellationToken);
    }
    public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(
    CancellationToken cancellationToken = default)
    {
        return await _context.GetRevenueSummaryAsync(cancellationToken);
    }

    public async Task<List<CategoryStatsDto>> GetCategoryStatsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.GetCategoryStatsAsync(cancellationToken);
    }
}