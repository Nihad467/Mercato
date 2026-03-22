using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Product.DTOs;

namespace Mercato.Application.Product.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, GetProductByIdDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetProductByIdDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.GetProductByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return null;

        return new GetProductByIdDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryName = product.Category.Name
        };
    }
}