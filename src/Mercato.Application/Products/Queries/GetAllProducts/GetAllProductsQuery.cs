using MediatR;
using Mercato.Application.Common.Models.Pagination;
using Mercato.Application.Products.DTOs;

namespace Mercato.Application.Products.Queries.GetAllProducts;

public class GetAllProductsQuery : IRequest<PagedResponse<ProductListItemDto>>
{
    public ProductQueryParameters Parameters { get; set; } = new();
}