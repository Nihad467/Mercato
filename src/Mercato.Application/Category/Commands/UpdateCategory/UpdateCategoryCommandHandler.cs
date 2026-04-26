using MediatR;
using Mercato.Application.Common.Caching;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Category.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public UpdateCategoryCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _context.GetCategoryByIdAsync(
            request.Category.Id,
            cancellationToken);

        if (category is null)
            return false;

        category.Name = request.Category.Name;

        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(
            CacheKeys.CategoriesList,
            cancellationToken);

        await _cacheService.RemoveAsync(
            CacheKeys.CategoryById(request.Category.Id),
            cancellationToken);

        return true;
    }
}