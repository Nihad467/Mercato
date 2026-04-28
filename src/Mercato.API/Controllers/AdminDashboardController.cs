using Mercato.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _adminDashboardService.GetSummaryAsync(cancellationToken);

        return Ok(summary);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] int take = 5,
        CancellationToken cancellationToken = default)
    {
        var products = await _adminDashboardService.GetTopProductsAsync(
            take,
            cancellationToken);

        return Ok(products);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts(
        [FromQuery] int threshold = 5,
        CancellationToken cancellationToken = default)
    {
        var products = await _adminDashboardService.GetLowStockProductsAsync(
            threshold,
            cancellationToken);

        return Ok(products);
    }

    [HttpGet("recent-orders")]
    public async Task<IActionResult> GetRecentOrders(
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var orders = await _adminDashboardService.GetRecentOrdersAsync(
            take,
            cancellationToken);

        return Ok(orders);
    }
    [HttpGet("revenue-summary")]
    public async Task<IActionResult> GetRevenueSummary(
    CancellationToken cancellationToken = default)
    {
        var summary = await _adminDashboardService.GetRevenueSummaryAsync(cancellationToken);

        return Ok(summary);
    }

    [HttpGet("category-stats")]
    public async Task<IActionResult> GetCategoryStats(
        CancellationToken cancellationToken = default)
    {
        var stats = await _adminDashboardService.GetCategoryStatsAsync(cancellationToken);

        return Ok(stats);
    }
}