using MediatR;
using Mercato.Application.Category.DTOs;
using Mercato.Application.Common.Caching;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Category.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, GetCategoryByIdDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetCategoryByIdQueryHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<GetCategoryByIdDto?> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.CategoryById(request.Id);

        var cachedCategory = await _cacheService.GetAsync<GetCategoryByIdDto>(
            cacheKey,
            cancellationToken);

        if (cachedCategory is not null)
            return cachedCategory;

        var category = await _context.GetCategoryByIdAsync(
            request.Id,
            cancellationToken);

        if (category is null)
            return null;

        var result = new GetCategoryByIdDto
        {
            Id = category.Id,
            Name = category.Name
        };

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(30),
            cancellationToken);

        return result;
    }
}