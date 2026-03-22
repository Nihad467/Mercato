using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.GetProductByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return false;

        _context.RemoveProduct(product);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}