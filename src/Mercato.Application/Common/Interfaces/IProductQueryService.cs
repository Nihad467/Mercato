using Mercato.Application.Common.Models.Pagination;
using Mercato.Application.Products.DTOs;

namespace Mercato.Application.Common.Interfaces;

public interface IProductQueryService
{
    Task<PagedResponse<ProductListItemDto>> GetPagedAsync(
        ProductQueryParameters parameters,
        CancellationToken cancellationToken);
}