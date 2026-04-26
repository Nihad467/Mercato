using MediatR;
using Mercato.Application.Category.DTOs;
using Mercato.Application.Common.Caching;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Category.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<GetAllCategoriesDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetAllCategoriesQueryHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<List<GetAllCategoriesDto>> Handle(
        GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var cachedCategories = await _cacheService.GetAsync<List<GetAllCategoriesDto>>(
            CacheKeys.CategoriesList,
            cancellationToken);

        if (cachedCategories is not null)
            return cachedCategories;

        var categories = await _context.GetAllCategoriesAsync(cancellationToken);

        var result = categories.Select(category => new GetAllCategoriesDto
        {
            Id = category.Id,
            Name = category.Name
        }).ToList();

        await _cacheService.SetAsync(
            CacheKeys.CategoriesList,
            result,
            TimeSpan.FromMinutes(30),
            cancellationToken);

        return result;
    }
}