using MediatR;
using Mercato.Application.Common.Caching;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Product.Commands.CreateProduct;
using Mercato.Domain.Entities;

namespace Mercato.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICacheService _cacheService;

    public CreateProductCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorageService,
        ICacheService cacheService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _cacheService = cacheService;
    }

    public async Task<int> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Product;

        var categoryExists = await _context.CategoryExistsAsync(
            dto.CategoryId,
            cancellationToken);

        if (!categoryExists)
            throw new Exception("Verilən CategoryId mövcud deyil.");

        var product = new Mercato.Domain.Entities.Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId
        };

        if (dto.Images is not null && dto.Images.Count > 0)
        {
            for (int i = 0; i < dto.Images.Count; i++)
            {
                var image = dto.Images[i];

                using var stream = image.OpenReadStream();

                var objectKey = await _fileStorageService.SaveAsync(
                    stream,
                    image.FileName,
                    image.ContentType,
                    cancellationToken);

                product.Images.Add(new ProductImage
                {
                    ObjectKey = objectKey,
                    IsMain = i == 0,
                    Order = i
                });
            }
        }

        await _context.AddProductAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(
            CacheKeys.ProductsListPrefix,
            cancellationToken);

        return product.Id;
    }
}