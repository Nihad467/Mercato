using Mercato.Application.Admin.Dashboard.Dtos;

namespace Mercato.Application.Common.Interfaces;

public interface IAdminDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);

    Task<List<TopProductDto>> GetTopProductsAsync(
        int take,
        CancellationToken cancellationToken = default);

    Task<List<LowStockProductDto>> GetLowStockProductsAsync(
        int threshold,
        CancellationToken cancellationToken = default);

    Task<List<RecentOrderDto>> GetRecentOrdersAsync(
        int take,
        CancellationToken cancellationToken = default);

    Task<RevenueSummaryDto> GetRevenueSummaryAsync(
        CancellationToken cancellationToken = default);

    Task<List<CategoryStatsDto>> GetCategoryStatsAsync(
        CancellationToken cancellationToken = default);
}