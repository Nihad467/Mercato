using System.Text.Json;
using MediatR;
using Mercato.Application.Common.Caching;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Common.Models.Pagination;
using Mercato.Application.Products.DTOs;

namespace Mercato.Application.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler
    : IRequestHandler<GetAllProductsQuery, PagedResponse<ProductListItemDto>>
{
    private readonly IProductQueryService _productQueryService;
    private readonly ICacheService _cacheService;

    public GetAllProductsQueryHandler(
        IProductQueryService productQueryService,
        ICacheService cacheService)
    {
        _productQueryService = productQueryService;
        _cacheService = cacheService;
    }

    public async Task<PagedResponse<ProductListItemDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = CreateCacheKey(request);

        var cachedProducts = await _cacheService.GetAsync<PagedResponse<ProductListItemDto>>(
            cacheKey,
            cancellationToken);

        if (cachedProducts is not null)
            return cachedProducts;

        var result = await _productQueryService.GetPagedAsync(
            request.Parameters,
            cancellationToken);

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(10),
            cancellationToken);

        return result;
    }

    private static string CreateCacheKey(GetAllProductsQuery request)
    {
        var parametersJson = JsonSerializer.Serialize(request.Parameters);

        return $"{CacheKeys.ProductsListPrefix}:{parametersJson}";
    }
}