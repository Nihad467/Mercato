using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Product.Commands.CreateProduct;

namespace Mercato.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Product;

        var categoryExists = await _context.CategoryExistsAsync(dto.CategoryId, cancellationToken);

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

        await _context.AddProductAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}