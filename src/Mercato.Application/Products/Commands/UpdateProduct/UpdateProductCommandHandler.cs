using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Product.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Product;

        var product = await _context.GetProductByIdAsync(dto.Id, cancellationToken);

        if (product is null)
            return false;

        var categoryExists = await _context.CategoryExistsAsync(dto.CategoryId, cancellationToken);

        if (!categoryExists)
            throw new Exception("Verilən CategoryId mövcud deyil.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}