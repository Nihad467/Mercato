using MediatR;
using Mercato.Application.Product.DTOs;

namespace Mercato.Application.Product.Queries.GetAllProducts;

public record GetAllProductsQuery : IRequest<List<GetAllProductsDto>>;