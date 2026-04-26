using MediatR;
using Mercato.Application.Common.Caching;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Category.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteCategoryCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cacheService = cacheService;
    }

    public async Task<DeleteCategoryResult> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _context.GetCategoryByIdAsync(
            request.Id,
            cancellationToken);

        if (category is null)
        {
            return new DeleteCategoryResult
            {
                IsSuccess = false,
                Message = "Category tapılmadı."
            };
        }

        var hasProducts = await _context.CategoryHasProductsAsync(
            request.Id,
            cancellationToken);

        if (hasProducts)
        {
            return new DeleteCategoryResult
            {
                IsSuccess = false,
                Message = "Bu category silinə bilməz, ona aid productlar var."
            };
        }

        _context.RemoveCategory(category);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(
            CacheKeys.CategoriesList,
            cancellationToken);

        await _cacheService.RemoveAsync(
            CacheKeys.CategoryById(request.Id),
            cancellationToken);

        return new DeleteCategoryResult
        {
            IsSuccess = true,
            Message = "Category uğurla silindi."
        };
    }
}