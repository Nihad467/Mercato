using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Product.Commands.UpdateProduct;

namespace Mercato.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, int>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Product;

        var product = await _context.GetProductByIdAsync(dto.Id, cancellationToken);

        if (product is null)
            throw new Exception("Product not found");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}