using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Common.Models.Pagination;
using Mercato.Application.Products.DTOs;

namespace Mercato.Application.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler
    : IRequestHandler<GetAllProductsQuery, PagedResponse<ProductListItemDto>>
{
    private readonly IProductQueryService _productQueryService;

    public GetAllProductsQueryHandler(IProductQueryService productQueryService)
    {
        _productQueryService = productQueryService;
    }

    public async Task<PagedResponse<ProductListItemDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        return await _productQueryService.GetPagedAsync(
            request.Parameters,
            cancellationToken);
    }
}