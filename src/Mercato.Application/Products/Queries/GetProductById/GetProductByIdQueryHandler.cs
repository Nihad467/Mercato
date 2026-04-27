using MediatR;
using Mercato.Application.Common.Caching;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Product.DTOs;

namespace Mercato.Application.Product.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, GetProductByIdDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetProductByIdQueryHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<GetProductByIdDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.ProductById(request.Id);

        var cachedProduct = await _cacheService.GetAsync<GetProductByIdDto>(
            cacheKey,
            cancellationToken);

        if (cachedProduct is not null)
            return cachedProduct;

        var product = await _context.GetProductByIdAsync(
            request.Id,
            cancellationToken);

        if (product is null)
            return null;

        var result = new GetProductByIdDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryName = product.Category.Name
        };

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(10),
            cancellationToken);

        return result;
    }
}