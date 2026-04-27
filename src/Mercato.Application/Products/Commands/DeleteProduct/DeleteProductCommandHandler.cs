using MediatR;
using Mercato.Application.Common.Caching;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteProductCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _context.GetProductByIdAsync(
            request.Id,
            cancellationToken);

        if (product is null)
            return false;

        _context.RemoveProduct(product);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(
            CacheKeys.ProductsListPrefix,
            cancellationToken);

        await _cacheService.RemoveAsync(
            CacheKeys.ProductById(request.Id),
            cancellationToken);

        return true;
    }
}