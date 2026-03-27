namespace Mercato.Infrastructure.Services;

using global::Mercato.Application.Common.Interfaces;
using global::Mercato.Application.Common.Models.Pagination;
using global::Mercato.Application.Products.DTOs;
using global::Mercato.Domain.Entities;
using global::Mercato.Infrastructure.Persistence.Context;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Common.Models.Pagination;
using Mercato.Application.Products.DTOs;
using Mercato.Domain.Entities;
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

        query = ApplySorting(query, parameters.SortBy, parameters.Descending);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
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
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling((double)totalCount / parameters.PageSize)
        };
    }

    private static IQueryable<Product> ApplySorting(
        IQueryable<Product> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.Trim().ToLower() switch
        {
            "name" => descending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),

            "price" => descending
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),

            "stock" => descending
                ? query.OrderByDescending(p => p.Stock)
                : query.OrderBy(p => p.Stock),

            "category" => descending
                ? query.OrderByDescending(p => p.Category.Name)
                : query.OrderBy(p => p.Category.Name),

            _ => descending
                ? query.OrderByDescending(p => p.Id)
                : query.OrderBy(p => p.Id)
        };
    }
}
