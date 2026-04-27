using MediatR;
using Mercato.Application.Common.Caching;
using Mercato.Application.Common.Interfaces;
using System.Net.Sockets;

namespace Mercato.Application.Product.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICacheService _cacheService;

    public UpdateProductCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorageService,
        ICacheService cacheService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _cacheService = cacheService;
    }

    public async Task<int> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Product;

        var product = await _context.GetProductByIdAsync(
            dto.Id,
            cancellationToken);

        if (product is null)
            throw new Exception("Product tapılmadı.");

        var categoryExists = await _context.CategoryExistsAsync(
            dto.CategoryId,
            cancellationToken);

        if (!categoryExists)
            throw new Exception("Verilən CategoryId mövcud deyil.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;

        if (dto.Images is not null && dto.Images.Count > 0)
        {
            foreach (var oldImage in product.Images.ToList())
            {
                await _fileStorageService.DeleteFileAsync(
                    oldImage.ObjectKey,
                    cancellationToken);
            }

            _context.RemoveProductImages(product.Images.ToList());
            product.Images.Clear();

            for (int i = 0; i < dto.Images.Count; i++)
            {
                var image = dto.Images[i];

                using var stream = image.OpenReadStream();

                var objectKey = await _fileStorageService.SaveAsync(
                    stream,
                    image.FileName,
                    image.ContentType,
                    cancellationToken);

                product.Images.Add(new Mercato.Domain.Entities.ProductImage
                {
                    ProductId = product.Id,
                    ObjectKey = objectKey,
                    IsMain = i == 0,
                    Order = i
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(
            CacheKeys.ProductsListPrefix,
            cancellationToken);

        await _cacheService.RemoveAsync(
            CacheKeys.ProductById(product.Id),
            cancellationToken);

        return product.Id;
    }
}