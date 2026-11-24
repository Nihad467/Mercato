namespace Mercato.Infrastructure.Services;

using Mercato.Application.Common.Interfaces;
using Mercato.Application.Common.Models.Pagination;
using Mercato.Application.Products.DTOs;
using Mercato.Domain.Entities;
using Mercato.Domain.Enums;
using Mercato.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;


public class ProductQueryService : IProductQueryService
{
    private readonly MercatoDbContext _context;

    public ProductQueryService(MercatoDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<ProductListItemDto>> GetPagedAsync(
        ProductQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        IQueryable<Product> query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category);

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == parameters.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchTerm = parameters.Search.Trim();
            query = query.Where(p => p.Name.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling((double)totalCount / parameters.PageSize);
        var pageNumber = totalPages == 0
            ? 1
            : Math.Min(parameters.PageNumber, totalPages);

        query = ApplySorting(query, parameters.SortBy, parameters.Descending);

        var items = await query
            .Skip((pageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<ProductListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    private IQueryable<Product> ApplySorting(
        IQueryable<Product> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.Trim().ToLower() switch
        {
            "name" => descending
                ? query.OrderByDescending(p => p.Name).ThenBy(p => p.Id)
                : query.OrderBy(p => p.Name).ThenBy(p => p.Id),

            "price" => descending
                ? query.OrderByDescending(p => p.Price).ThenBy(p => p.Id)
                : query.OrderBy(p => p.Price).ThenBy(p => p.Id),

            "stock" => descending
                ? query.OrderByDescending(p => p.Stock).ThenBy(p => p.Id)
                : query.OrderBy(p => p.Stock).ThenBy(p => p.Id),

            "category" => descending
                ? query.OrderByDescending(p => p.Category.Name).ThenBy(p => p.Id)
                : query.OrderBy(p => p.Category.Name).ThenBy(p => p.Id),

            "popular" => descending
                ? query.OrderByDescending(p => _context.OrderItems
                    .Where(item => item.ProductId == p.Id && item.Order.Status == OrderStatus.Paid)
                    .Select(item => (int?)item.Quantity)
                    .Sum() ?? 0).ThenBy(p => p.Id)
                : query.OrderBy(p => _context.OrderItems
                    .Where(item => item.ProductId == p.Id && item.Order.Status == OrderStatus.Paid)
                    .Select(item => (int?)item.Quantity)
                    .Sum() ?? 0).ThenBy(p => p.Id),

            _ => descending
                ? query.OrderByDescending(p => p.Id)
                : query.OrderBy(p => p.Id)
        };
    }
}
