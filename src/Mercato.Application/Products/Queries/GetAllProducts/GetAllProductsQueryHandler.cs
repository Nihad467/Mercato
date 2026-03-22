using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Product.DTOs;

namespace Mercato.Application.Product.Queries.GetAllProducts;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<GetAllProductsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GetAllProductsDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.GetAllProductsAsync(cancellationToken);

        return products.Select(product => new GetAllProductsDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock
        }).ToList();
    }
}