using MediatR;
using Mercato.Application.Common.Caching;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Category.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public CreateCategoryCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<int> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = new Mercato.Domain.Entities.Category
        {
            Name = request.Category.Name
        };

        await _context.AddCategoryAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(
            CacheKeys.CategoriesList,
            cancellationToken);

        return category.Id;
    }
}